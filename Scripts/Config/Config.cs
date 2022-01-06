using System.Linq;
/// <summary>
/// Sisältää eri tietoja pelaajasta mm. hiiriasetukset, crosshairasetukset ja aliaksen
/// </summary>
namespace Assets.Scripts.Config
{
    class Config
    {
        public ConfigDirective[] configValues { get; set; } = new ConfigDirective[]
        {
            new ConfigDirective()
            {
                Key="MouseSensitivity",
                Value="1,55",
                Default="1,55"
            },
            new ConfigDirective()
            {
                Key="PlayerName",
                Value="",
                Default=""
            },
            new ConfigDirective()
            {
                Key="RedCrossHair",
                Value="255",
                Default="255"
            },
            new ConfigDirective()
            {
                Key="GreenCrossHair",
                Value="0",
                Default="0"
            },
            new ConfigDirective
            {
                Key="BlueCrossHair",
                Value="0",
                Default="0"
            },
            new ConfigDirective
            {
                Key="AlphaCrossHair",
                Value="125",
                Default="125"
            },
            new ConfigDirective
            {
                Key="CrossYSize",
                Value="25",
                Default="25"
            },
            new ConfigDirective
            {
                Key="CrossXSize",
                Value="25",
                Default="25"
            },
            new ConfigDirective
            {
                Key="CrossHToggle",
                Value="true",
                Default="true"
            },
            new ConfigDirective
            {
                Key="CrossVToggle",
                Value="true",
                Default="true"
            },
            new ConfigDirective
            {
                Key="CrossMToggle",
                Value="true",
                Default="true"
            }
        };
        public ConfigDirective getConfigDirective(string key)
        {
            return configValues.FirstOrDefault(x => x.Key == key);
        }
        public void setConfigDirective(string key, string value)
        {
            getConfigDirective(key).Value = value;
        }
    }
}
