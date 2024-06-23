using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nadena.dev.ndmf;
using UnityEngine;

namespace io.github.azukimochi
{
    [ParameterProviderFor(typeof(LightLimitChangerSettings))]
    internal sealed class ParameterProvider : IParameterProvider
    {
        private LightLimitChangerSettings instance;

        public ParameterProvider(LightLimitChangerSettings instance) => this.instance = instance;

        public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null)
        {
            if (instance == null)
                yield break;

            var parameters = instance.Parameters;

            yield return Parameter<bool>(Passes.ParameterName_Toggle);

            if (!parameters.IsSeparateLightControl)
            {
                yield return Parameter<float>(Passes.ParameterName_Value);
            }
            else
            {
                yield return Parameter<float>(Passes.ParameterName_Min);
                yield return Parameter<float>(Passes.ParameterName_Max);
            }

            if (parameters.AllowColorTempControl)
                yield return Parameter<float>(Passes.ParameterName_ColorTemp);

            if (parameters.AllowSaturationControl)
                yield return Parameter<float>(Passes.ParameterName_Saturation);

            if (parameters.AllowMonochromeControl)
                yield return Parameter<float>(Passes.ParameterName_Monochrome);

            if (parameters.AllowUnlitControl)
                yield return Parameter<float>(Passes.ParameterName_Unlit);

            if (parameters.AllowEmissionControl)
                yield return Parameter<float>(ParameterName_Emission);
        }

        private ProvidedParameter Parameter<T>(string name, bool sync = true, ParameterNamespace @namespace = ParameterNamespace.Animator) 
            => Parameter(name, sync, @namespace, typeof(T) == typeof(int) ? AnimatorControllerParameterType.Int : typeof(T) == typeof(float) ? AnimatorControllerParameterType.Float : typeof(T) == typeof(bool) ? AnimatorControllerParameterType.Bool : default);

        private ProvidedParameter Parameter(string name, bool sync = true, ParameterNamespace @namespace = ParameterNamespace.Animator, AnimatorControllerParameterType? type = null)
            => new ProvidedParameter(name, @namespace, instance, PluginDefinition.Instance, type)
            {
                WantSynced = sync,
            };

        public void RemapParameters(ref ImmutableDictionary<(ParameterNamespace, string), ParameterMapping> nameMap, BuildContext context = null)
        {

        }
    }
}
