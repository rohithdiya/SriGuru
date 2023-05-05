﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using VedAstro.Library;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace API
{
    /// <summary>
    /// API Functions related to Person Profiles
    /// </summary>
    public class PersonAPI
    {



        /// <summary>
        /// Generate a human readable Person ID
        /// 
        /// </summary>
        [Function("GetNewPersonId")]
        public static async Task<HttpResponseData> GetNewPersonId([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData incomingRequest)
        {

            try
            {
                //STAGE 1 : GET DATA OUT
                //data out of request
                var rootXml = await APITools.ExtractDataFromRequestXml(incomingRequest);
                var name = rootXml.Element("Name")?.Value;
                var birthYear = rootXml.Element("BirthYear")?.Value;

                //special ID made for human brains
                var brandNewHumanReadyID = await APITools.GeneratePersonId(name, birthYear);

                return APITools.PassMessage(brandNewHumanReadyID, incomingRequest);

            }
            catch (Exception e)
            {
                //log error
                await APILogger.Error(e, incomingRequest);

                //format error nicely to show user
                return APITools.FailMessage(e, incomingRequest);
            }

        }

        [Function("addperson")]
        public static async Task<HttpResponseData> AddPerson([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData incomingRequest)
        {

            try
            {
                //get new person data out of incoming request
                //note: inside new person xml already contains user id
                var newPersonXml = await APITools.ExtractDataFromRequestXml(incomingRequest);

                //add new person to main list
                await APITools.AddXElementToXDocumentAzure(newPersonXml, APITools.PersonListFile, APITools.BlobContainerName);

                return APITools.PassMessage(incomingRequest);

            }
            catch (Exception e)
            {
                //log error
                await APILogger.Error(e, incomingRequest);

                //format error nicely to show user
                return APITools.FailMessage(e, incomingRequest);
            }

        }

        /// <summary>
        /// Gets person all details from only hash
        /// </summary>
        [Function("getperson")]
        public static async Task<HttpResponseData> GetPerson([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData incomingRequest)
        {

            try
            {
                //get hash that will be used find the person
                var requestData = await APITools.ExtractDataFromRequestXml(incomingRequest);
                var personId = requestData.Value;

                //get the person record by ID
                var foundPerson = await APITools.FindPersonXMLById(personId);

                //send person to caller
                return APITools.PassMessage(foundPerson, incomingRequest);

            }
            catch (Exception e)
            {
                //log error
                await APILogger.Error(e, incomingRequest);

                //let caller know fail, include exception info for easy debugging
                return APITools.FailMessage(e, incomingRequest);
            }


        }

        /// <summary>
        /// Updates a person's record, uses hash to identify person to overwrite
        /// </summary>
        [Function("updateperson")]
        public static async Task<HttpResponseData> UpdatePerson(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData incomingRequest)
        {

            try
            {
                //get unedited hash & updated person details from incoming request
                var updatedPersonXml = await APITools.ExtractDataFromRequestXml(incomingRequest);

                //save a copy of the original person record in recycle bin, just in-case accidental update
                var personId = Person.FromXml(updatedPersonXml).Id;//does not change
                var originalPerson = await APITools.GetPersonById(personId);
                await APITools.AddXElementToXDocumentAzure(originalPerson.ToXml(), APITools.RecycleBinFile, APITools.BlobContainerName);

                //directly updates and saves new person record to main list (does all the work, sleep easy)
                await APITools.UpdatePersonRecord(updatedPersonXml);

                //all is good baby
                return APITools.PassMessage(incomingRequest);

            }
            catch (Exception e)
            {
                //log error
                await APILogger.Error(e, incomingRequest);

                //format error nicely to show user
                return APITools.FailMessage(e, incomingRequest);
            }

        }

        /// <summary>
        /// Deletes a person's record, uses hash to identify person
        /// Note : user id is not checked here because Person hash
        /// can't even be generated by client side if you don't have access.
        /// Theoretically anybody who gets the hash of the person,
        /// can delete the record by calling this API
        /// </summary>
        [Function(nameof(DeletePerson))]
        public static async Task<HttpResponseData> DeletePerson([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData incomingRequest)
        {

            try
            {
                //get unedited hash & updated person details from incoming request
                var requestData = await APITools.ExtractDataFromRequestXml(incomingRequest);
                var personId = requestData.Value;

                //get the person record that needs to be deleted
                var personToDelete = await APITools.FindPersonXMLById(personId);

                //add deleted person to recycle bin 
                await APITools.AddXElementToXDocumentAzure(personToDelete, APITools.RecycleBinFile, APITools.BlobContainerName);

                //delete the person record,
                await APITools.DeleteXElementFromXDocumentAzure(personToDelete, APITools.PersonListFile, APITools.BlobContainerName);

                return APITools.PassMessage(incomingRequest);

            }
            catch (Exception e)
            {
                //log error
                await APILogger.Error(e, incomingRequest);

                //format error nicely to show user
                return APITools.FailMessage(e, incomingRequest);
            }
        }

        /// <summary>
        /// Gets all person profiles owned by User ID & Visitor ID
        /// </summary>
        [Function(nameof(GetPersonList))]
        public static async Task<HttpResponseData> GetPersonList([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData incomingRequest)
        {
            AppInstance.IncomingRequest = incomingRequest;
            var personListFileUrl = APITools.PersonListFile;

            try
            {

                //STAGE 1 : GET DATA OUT
                //data out of request
                var rootXml = await APITools.ExtractDataFromRequestXml(incomingRequest);
                var userId = rootXml.Element("UserId")?.Value;
                var visitorId = rootXml.Element("VisitorId")?.Value;

                //STAGE 2 : SWAP DATA
                //swap visitor ID with user ID if any (data follows user when log in)
                await APITools.SwapUserId(visitorId, userId, personListFileUrl);

                //STAGE 3 : FILTER 
                //get latest all match reports
                var personListXml = await APITools.GetXmlFileFromAzureStorage(personListFileUrl, APITools.BlobContainerName);
                //filter out record by user id
                var userIdList = Tools.FindXmlByUserId(personListXml, userId);

                //STAGE 4 : SEND XML todo JSON 
                //convert list to xml
                var xmlPayload = Tools.AnyTypeToXmlList(userIdList);

                //send filtered list to caller
                return APITools.PassMessage(xmlPayload, incomingRequest);

            }
            catch (Exception e)
            {
                //log error
                await APILogger.Error(e, incomingRequest);
                //format error nicely to show user
                return APITools.FailMessage(e, incomingRequest);
            }

        }

        /// <summary>
        /// Gets all person profiles owned by User ID & Visitor ID
        /// </summary>
        [Function(nameof(Account))]
        public static async Task<HttpResponseData> Account(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Account/UserID/{userId}/VisitorID/{visitorId}")] HttpRequestData incomingRequest,
            string userId,
            string visitorId)
        {
        //used when visitor id person were moved to user id, shouldn't happen all the time, obviously adds to the lag (needs speed testing) 
        TryAgain:

            try
            {

                //get all person list from storage
                var personListXml = await APITools.GetXmlFileFromAzureStorage(APITools.PersonListFile, APITools.BlobContainerName);

                //filter out person by user id
                var userIdPersonList = Tools.FindXmlByUserId(personListXml, userId);

                //filter out person by visitor id
                var visitorIdPersonList = Tools.FindXmlByUserId(personListXml, visitorId);

                //before sending to user, clean the data
                //if user made profile while logged out then logs in, transfer the profiles created with visitor id to the new user id
                //if this is not done, then when user loses the visitor ID, they also loose access to the person profile
                var loggedIn = userId != "101" && !(string.IsNullOrEmpty(userId));//already logged in if true
                var visitorProfileExists = visitorIdPersonList.Any();
                if (loggedIn && visitorProfileExists)
                {
                    //transfer to user id
                    foreach (var person in visitorIdPersonList)
                    {
                        //edit data direct to for speed
                        person.Element("UserId").Value = userId;

                        //save to main list
                        await APITools.UpdatePersonRecord(person);
                    }

                    //after the transfer, restart the call as though new, so that user only gets the correct list at all times (though this might be little slow)
                    goto TryAgain;
                }

                //STAGE 4 : combine and remove duplicates
                if (visitorIdPersonList.Any()) { userIdPersonList.AddRange(visitorIdPersonList); }
                List<XElement> personListNoDupes = userIdPersonList.Distinct().ToList();

                var x = Tools.ListToJson<XElement>(personListNoDupes);

                //send filtered list to caller
                return APITools.PassMessageJson(x, incomingRequest);

            }
            catch (Exception e)
            {
                //log error
                await APILogger.Error(e, incomingRequest);
                //format error nicely to show user
                return APITools.FailMessageJson(e, incomingRequest);
            }


        }


    }
}
