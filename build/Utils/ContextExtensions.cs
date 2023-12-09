using Cake.Common;
using Cake.Core;

namespace build.Utils;

internal static class ContextExtensions
{
    public static string Arg(this ICakeContext context, string name)
    {
        var envVar = context.EnvironmentVariable(name.ToUpperSnakeCase());
        return envVar ?? context.Argument<string>(name);
    }
}