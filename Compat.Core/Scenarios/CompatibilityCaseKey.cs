using System;

namespace Compat.Core
{
    /// <summary>
    /// Builds a <see cref="CompatibilityCase.Key"/> from an action name and a scenario constant
    /// (see <see cref="CompatibilityScenarios"/> or a feature-local scenarios class), instead of
    /// scattering "$"{action}/{scenario}"" string concatenation across cases files.
    /// Does not change the key FORMAT ("Action/scenario") — only centralizes how it is built.
    /// </summary>
    public static class CompatibilityCaseKey
    {
        public static string For(string action, string scenario)
        {
            if (string.IsNullOrEmpty(action)) throw new ArgumentException("action is required", nameof(action));
            if (string.IsNullOrEmpty(scenario)) throw new ArgumentException("scenario is required", nameof(scenario));
            return action + "/" + scenario;
        }
    }
}
