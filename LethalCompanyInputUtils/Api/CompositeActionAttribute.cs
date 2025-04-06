using System;
using LethalCompanyInputUtils.Api.Composite;

namespace LethalCompanyInputUtils.Api;


public abstract class CompositeActionAttribute : Attribute
{
    internal abstract void BuildInto(InputActionBindingBuilder actionBuilder);
}