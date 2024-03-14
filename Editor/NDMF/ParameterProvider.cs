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
        }

        private ProvidedParameter Parameter<T>(string name, ParameterNamespace @namespace = ParameterNamespace.Animator) 
            => Parameter(name, @namespace, typeof(T) == typeof(int) ? AnimatorControllerParameterType.Int : typeof(T) == typeof(float) ? AnimatorControllerParameterType.Float : typeof(T) == typeof(bool) ? AnimatorControllerParameterType.Bool : default);

        private ProvidedParameter Parameter(string name, ParameterNamespace @namespace = ParameterNamespace.Animator, AnimatorControllerParameterType? type = null) 
            => new ProvidedParameter(name, @namespace, instance, PluginDefinition.Instance, type);

        public void RemapParameters(ref ImmutableDictionary<(ParameterNamespace, string), ParameterMapping> nameMap, BuildContext context = null)
        {

        }
    }
}
