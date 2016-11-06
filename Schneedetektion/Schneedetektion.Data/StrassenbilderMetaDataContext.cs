using Schneedetektion.Data.Properties;

namespace Schneedetektion.Data
{
    public partial class StrassenbilderMetaDataContext
    {
        public StrassenbilderMetaDataContext() : base(Settings.Default.StrassenbilderMetaConnectionString, mappingSource)
        {
            OnCreated();
        }
    }
}
