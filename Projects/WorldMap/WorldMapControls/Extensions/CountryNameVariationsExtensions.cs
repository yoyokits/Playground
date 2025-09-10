// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace WorldMapControls.Extensions
{
    using System;
    using WorldMapControls.Models.Enums;

    /// <summary>
    /// Provides country name variations for GeoJSON mapping.
    /// </summary>
    public static class CountryNameVariationsExtensions
    {
        #region Methods

        /// <summary>
        /// Gets all possible name variations for a country that might appear in GeoJSON data.
        /// </summary>
        /// <param name="country">The country enum value</param>
        /// <returns>Array of possible country names</returns>
        public static string[] GetGeoJsonNameVariations(this Country country)
        {
            return country switch
            {
                Country.UnitedStates => new[] { "United States of America", "United States", "USA", "US", "America" },
                Country.UnitedKingdom => new[] { "United Kingdom", "UK", "Great Britain", "Britain", "England", "Scotland", "Wales", "Northern Ireland" },
                Country.DemocraticRepublicOfCongo => new[] { 
                    "Democratic Republic of the Congo", "Congo (Democratic Republic)", 
                    "DRC", "Congo-Kinshasa", "Zaire", "Congo, Dem. Rep.", "Congo DR", "Congo DRC",
                    "Dem. Rep. Congo", "Democratic Rep. Congo" 
                },
                Country.RepublicOfCongo => new[] { 
                    "Republic of the Congo", "Congo", "Congo-Brazzaville", "Congo, Rep.", "Congo Republic" 
                },
                Country.CentralAfricanRepublic => new[] { 
                    "Central African Republic", "CAR", "Central African Rep." 
                },
                Country.SouthAfrica => new[] { "South Africa", "Republic of South Africa", "RSA" },
                Country.IvoryCoast => new[] { "Ivory Coast", "Côte d'Ivoire", "Cote d'Ivoire" },
                Country.UAE => new[] { "United Arab Emirates", "UAE" },
                Country.SaudiArabia => new[] { "Saudi Arabia", "Kingdom of Saudi Arabia" },
                Country.NorthKorea => new[] { "North Korea", "Democratic People's Republic of Korea", "DPRK" },
                Country.SouthKorea => new[] { "South Korea", "Republic of Korea", "Korea, South" },
                Country.NewZealand => new[] { "New Zealand", "NZ" },
                Country.PapuaNewGuinea => new[] { "Papua New Guinea", "PNG" },
                Country.SriLanka => new[] { "Sri Lanka", "Ceylon" },
                Country.Myanmar => new[] { "Myanmar", "Burma" },
                Country.CzechRepublic => new[] { "Czech Republic", "Czechia" },
                Country.NorthMacedonia => new[] { "North Macedonia", "Macedonia", "Republic of North Macedonia", "FYROM" },
                Country.BosniaAndHerzegovina => new[] { "Bosnia and Herzegovina", "Bosnia & Herzegovina", "BiH" },
                Country.SaoTomeAndPrincipe => new[] { "Sao Tome and Principe", "São Tomé and Príncipe" },
                Country.TimorLeste => new[] { "Timor-Leste", "East Timor", "Democratic Republic of Timor-Leste" },

                // Caribbean countries
                Country.Jamaica => new[] { "Jamaica" },
                Country.Cuba => new[] { "Cuba", "Republic of Cuba" },
                Country.Haiti => new[] { "Haiti", "Republic of Haiti" },
                Country.DominicanRepublic => new[] { "Dominican Republic", "DR" },

                // African countries (enhanced with more variations)
                Country.BurkinaFaso => new[] { "Burkina Faso" },
                Country.Guinea => new[] { "Guinea", "Republic of Guinea" },
                Country.GuineaBissau => new[] { "Guinea-Bissau", "Guinea Bissau" },
                Country.SierraLeone => new[] { "Sierra Leone" },
                Country.EquatorialGuinea => new[] { "Equatorial Guinea" },
                Country.SouthSudan => new[] { 
                    "South Sudan", "Republic of South Sudan", "S. Sudan", "S Sudan" 
                },
                Country.Ethiopia => new[] { "Ethiopia", "Federal Democratic Republic of Ethiopia" },
                Country.Tanzania => new[] { "Tanzania", "United Republic of Tanzania" },
                Country.Kenya => new[] { "Kenya", "Republic of Kenya" },
                Country.Uganda => new[] { "Uganda", "Republic of Uganda" },
                Country.Rwanda => new[] { "Rwanda", "Republic of Rwanda" },
                Country.Burundi => new[] { "Burundi", "Republic of Burundi" },
                Country.Chad => new[] { "Chad", "Republic of Chad" },
                Country.Sudan => new[] { 
                    "Sudan", "Republic of the Sudan", "Republic of Sudan" 
                },
                Country.Libya => new[] { "Libya", "State of Libya" },
                Country.Tunisia => new[] { "Tunisia", "Republic of Tunisia" },
                Country.Algeria => new[] { "Algeria", "People's Democratic Republic of Algeria" },
                Country.Morocco => new[] { "Morocco", "Kingdom of Morocco" },
                Country.Ghana => new[] { "Ghana", "Republic of Ghana" },
                Country.Mali => new[] { "Mali", "Republic of Mali" },
                Country.Niger => new[] { "Niger", "Republic of the Niger" },
                Country.Senegal => new[] { "Senegal", "Republic of Senegal" },
                Country.Liberia => new[] { "Liberia", "Republic of Liberia" },
                Country.Gambia => new[] { "Gambia", "The Gambia", "Republic of The Gambia" },
                Country.Mauritania => new[] { "Mauritania", "Islamic Republic of Mauritania" },
                Country.Madagascar => new[] { "Madagascar", "Republic of Madagascar" },
                Country.Mozambique => new[] { "Mozambique", "Republic of Mozambique" },
                Country.Zimbabwe => new[] { "Zimbabwe", "Republic of Zimbabwe" },
                Country.Botswana => new[] { "Botswana", "Republic of Botswana" },
                Country.Namibia => new[] { "Namibia", "Republic of Namibia" },
                Country.Zambia => new[] { "Zambia", "Republic of Zambia" },
                Country.Malawi => new[] { "Malawi", "Republic of Malawi" },
                Country.Angola => new[] { "Angola", "Republic of Angola" },
                Country.Cameroon => new[] { "Cameroon", "Republic of Cameroon" },
                Country.Gabon => new[] { "Gabon", "Gabonese Republic" },
                Country.Nigeria => new[] { "Nigeria", "Federal Republic of Nigeria" },
                Country.Egypt => new[] { "Egypt", "Arab Republic of Egypt" },
                Country.Somalia => new[] { 
                    "Somalia", "Somali Republic", "Federal Republic of Somalia" 
                }, // Added Somalia variations

                // European countries (enhanced)
                Country.Germany => new[] { "Germany", "Federal Republic of Germany", "Deutschland" },
                Country.France => new[] { "France", "French Republic" },
                Country.Italy => new[] { "Italy", "Italian Republic" },
                Country.Spain => new[] { "Spain", "Kingdom of Spain" },
                Country.Portugal => new[] { "Portugal", "Portuguese Republic" },
                Country.Netherlands => new[] { "Netherlands", "Kingdom of the Netherlands", "Holland" },
                Country.Belgium => new[] { "Belgium", "Kingdom of Belgium" },
                Country.Switzerland => new[] { "Switzerland", "Swiss Confederation" },
                Country.Austria => new[] { "Austria", "Republic of Austria" },
                Country.Sweden => new[] { "Sweden", "Kingdom of Sweden" },
                Country.Norway => new[] { "Norway", "Kingdom of Norway" },
                Country.Finland => new[] { "Finland", "Republic of Finland" },
                Country.Denmark => new[] { "Denmark", "Kingdom of Denmark" },
                Country.Poland => new[] { "Poland", "Republic of Poland" },
                Country.Russia => new[] { "Russia", "Russian Federation", "USSR" },
                Country.Ukraine => new[] { "Ukraine" },
                Country.Turkey => new[] { "Turkey", "Republic of Turkey" },
                Country.Greece => new[] { "Greece", "Hellenic Republic" },
                Country.Romania => new[] { "Romania" },
                Country.Bulgaria => new[] { "Bulgaria", "Republic of Bulgaria" },
                Country.Hungary => new[] { "Hungary", "Republic of Hungary" },
                Country.Slovakia => new[] { "Slovakia", "Slovak Republic" },
                Country.Slovenia => new[] { "Slovenia", "Republic of Slovenia" },
                Country.Croatia => new[] { "Croatia", "Republic of Croatia" },
                Country.Serbia => new[] { "Serbia", "Republic of Serbia" },
                Country.Montenegro => new[] { "Montenegro" },
                Country.Albania => new[] { "Albania", "Republic of Albania" },
                Country.Estonia => new[] { "Estonia", "Republic of Estonia" },
                Country.Latvia => new[] { "Latvia", "Republic of Latvia" },
                Country.Lithuania => new[] { "Lithuania", "Republic of Lithuania" },
                Country.Belarus => new[] { "Belarus", "Republic of Belarus" },
                Country.Moldova => new[] { "Moldova", "Republic of Moldova" },
                Country.Azerbaijan => new[] { "Azerbaijan", "Republic of Azerbaijan" },
                Country.Iceland => new[] { "Iceland", "Republic of Iceland" },
                Country.Ireland => new[] { "Ireland", "Republic of Ireland", "Éire" },
                Country.Georgia => new[] { 
                    "Georgia", "Republic of Georgia" 
                }, // Added Georgia variations
                Country.Armenia => new[] { 
                    "Armenia", "Republic of Armenia" 
                }, // Added Armenia variations

                // Asian countries
                Country.China => new[] { "China", "People's Republic of China", "PRC" },
                Country.Japan => new[] { "Japan" },
                Country.India => new[] { "India", "Republic of India" },
                Country.Indonesia => new[] { "Indonesia", "Republic of Indonesia" },
                Country.Vietnam => new[] { "Vietnam", "Socialist Republic of Vietnam", "Viet Nam" },
                Country.Thailand => new[] { "Thailand", "Kingdom of Thailand" },
                Country.Malaysia => new[] { "Malaysia" },
                Country.Singapore => new[] { "Singapore", "Republic of Singapore" },
                Country.Philippines => new[] { "Philippines", "Republic of the Philippines" },
                Country.Cambodia => new[] { "Cambodia", "Kingdom of Cambodia" },
                Country.Laos => new[] { "Laos", "Lao People's Democratic Republic" },
                Country.Bangladesh => new[] { "Bangladesh", "People's Republic of Bangladesh" },
                Country.Pakistan => new[] { "Pakistan", "Islamic Republic of Pakistan" },
                Country.Afghanistan => new[] { "Afghanistan", "Islamic Republic of Afghanistan" },
                Country.Nepal => new[] { "Nepal", "Federal Democratic Republic of Nepal" },
                Country.Bhutan => new[] { "Bhutan", "Kingdom of Bhutan" },
                Country.Maldives => new[] { "Maldives", "Republic of Maldives" },
                Country.Mongolia => new[] { "Mongolia" },

                // Central Asian countries
                Country.Kazakhstan => new[] { "Kazakhstan", "Republic of Kazakhstan" },
                Country.Uzbekistan => new[] { "Uzbekistan", "Republic of Uzbekistan" },
                Country.Turkmenistan => new[] { 
                    "Turkmenistan", "Turkmen" 
                }, // Added Turkmenistan variations
                Country.Kyrgyzstan => new[] { "Kyrgyzstan", "Kyrgyz Republic" },
                Country.Tajikistan => new[] { "Tajikistan", "Republic of Tajikistan" },

                // Middle Eastern countries
                Country.Iran => new[] { "Iran", "Islamic Republic of Iran" },
                Country.Iraq => new[] { "Iraq", "Republic of Iraq" },
                Country.Israel => new[] { "Israel", "State of Israel" },
                Country.Palestine => new[] { "Palestine", "State of Palestine" },
                Country.Jordan => new[] { "Jordan", "Hashemite Kingdom of Jordan" },
                Country.Lebanon => new[] { "Lebanon", "Lebanese Republic" },
                Country.Syria => new[] { "Syria", "Syrian Arab Republic" },
                Country.Yemen => new[] { "Yemen", "Republic of Yemen" },
                Country.Oman => new[] { "Oman", "Sultanate of Oman" },
                Country.Qatar => new[] { "Qatar", "State of Qatar" },
                Country.Bahrain => new[] { "Bahrain", "Kingdom of Bahrain" },
                Country.Kuwait => new[] { "Kuwait", "State of Kuwait" },

                // American countries
                Country.Canada => new[] { "Canada" },
                Country.Mexico => new[] { "Mexico", "United Mexican States" },
                Country.Greenland => new[] { "Greenland", "Kalaallit Nunaat" },
                Country.Brazil => new[] { "Brazil", "Federative Republic of Brazil" },
                Country.Argentina => new[] { "Argentina", "Argentine Republic" },
                Country.Chile => new[] { "Chile", "Republic of Chile" },
                Country.Colombia => new[] { "Colombia", "Republic of Colombia" },
                Country.Venezuela => new[] { "Venezuela", "Bolivarian Republic of Venezuela" },
                Country.Peru => new[] { "Peru", "Republic of Peru" },
                Country.Ecuador => new[] { "Ecuador", "Republic of Ecuador" },
                Country.Bolivia => new[] { "Bolivia", "Plurinational State of Bolivia" },
                Country.Paraguay => new[] { "Paraguay", "Republic of Paraguay" },
                Country.Uruguay => new[] { "Uruguay", "Oriental Republic of Uruguay" },
                Country.Guyana => new[] { "Guyana", "Co-operative Republic of Guyana" },
                Country.Suriname => new[] { "Suriname", "Republic of Suriname" },

                // Oceania
                Country.Australia => new[] { "Australia", "Commonwealth of Australia" },
                Country.FijiIslands => new[] { "Fiji", "Republic of Fiji" },
                Country.SolomonIslands => new[] { "Solomon Islands" },
                Country.Vanuatu => new[] { "Vanuatu", "Republic of Vanuatu" },
                Country.NewCaledonia => new[] { "New Caledonia" },
                Country.Samoa => new[] { "Samoa", "Independent State of Samoa" },
                Country.Tonga => new[] { "Tonga", "Kingdom of Tonga" },
                Country.Palau => new[] { "Palau", "Republic of Palau" },
                Country.MarshallIslands => new[] { "Marshall Islands", "Republic of the Marshall Islands" },
                Country.Micronesia => new[] { "Micronesia", "Federated States of Micronesia" },
                Country.Kiribati => new[] { "Kiribati", "Republic of Kiribati" },
                Country.Nauru => new[] { "Nauru", "Republic of Nauru" },
                Country.Tuvalu => new[] { "Tuvalu" },

                // Default fallback
                _ => new[] { country.ToString() }
            };
        }

        #endregion Methods
    }
}