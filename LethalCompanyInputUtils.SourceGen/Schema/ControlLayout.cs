using System;
using Newtonsoft.Json;

namespace LethalCompanyInputUtils.SourceGen.Schema;

[Serializable]
public class ControlLayout
{
    public string Name { get; set; } = "";

    public string Extend { get; set; } = "";

    public Control[] Controls { get; set; } = [];
}