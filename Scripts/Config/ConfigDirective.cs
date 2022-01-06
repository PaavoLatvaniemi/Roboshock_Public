/// <summary>
/// Yksittäinen asetus configissa, jonka määrittelee avain ja arvo
/// </summary>
namespace Assets.Scripts.Config
{
    class ConfigDirective
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Default { get; set; }
    }
}
