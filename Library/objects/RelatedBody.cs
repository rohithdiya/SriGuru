﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Genso.Astrology.Library
{

    /// <summary>
    /// Simple class to wrap all related bodies (Planets, Houses & Signs)
    /// to Calculator Result
    /// </summary>
    public class RelatedBody : IToXml
    {
        public RelatedBody() { }

        /// <summary>
        /// List of planets related to this calculation result
        /// </summary>
        public List<PlanetName> RelatedPlanets { get; private set; } = new List<PlanetName>();

        /// <summary>
        /// List of Zodiac Signs related to this calculation result
        /// </summary>
        public List<ZodiacName> RelatedZodiac { get; private set; } = new List<ZodiacName>();

        /// <summary>
        /// List of houses related to this calculation result
        /// </summary>
        public List<HouseName> RelatedHouses { get; private set; } = new List<HouseName>();


        /// <summary>
        /// Converts all values to string seperated by comma
        /// Note: used when searching
        /// </summary>
        public override string ToString()
        {
            var compiledText = "";

            //this assumed that underlying type implements its own to string
            compiledText += Tools.ListToString(this.RelatedPlanets);
            compiledText += Tools.ListToString(this.RelatedHouses);
            compiledText += Tools.ListToString(this.RelatedZodiac);

            return compiledText;
        }

        public dynamic FromXml<T>(XElement relatedBodyXml) where T : IToXml => RelatedBody.FromXml(relatedBodyXml);

        public static RelatedBody FromXml(XElement relatedBodyXml)
        {
            var parsed = new RelatedBody();
            var relatedPlanetsXml = relatedBodyXml?.Element("PlanetNameList") ?? new XElement("PlanetNameList");
            parsed.RelatedPlanets = PlanetName.FromXmlList(relatedPlanetsXml);
            var relatedHousesXml = relatedBodyXml?.Element("HousesNameList") ?? new XElement("HousesNameList");
            parsed.RelatedHouses = HouseNameExtensions.FromXmlList(relatedHousesXml);
            var relatedZodiacXml = relatedBodyXml?.Element("ZodiacNameList") ?? new XElement("ZodiacNameList");
            parsed.RelatedZodiac = ZodiacNameExtensions.FromXmlList(relatedZodiacXml);

            return parsed;

        }



        public XElement ToXml()
        {
            //todo improve conformatiy if possible, PlanetName has to 
            var relatedPlanetsXml = PlanetName.ToXmlList(this.RelatedPlanets);
            var relatedHousesXml = HouseNameExtensions.ToXmlList(this.RelatedHouses);
            var relatedSignsXml = ZodiacNameExtensions.ToXmlList(this.RelatedZodiac);

            var returnXml = new XElement("RelatedBody", relatedPlanetsXml, relatedHousesXml, relatedSignsXml);
            return returnXml;

        }
    }
}
