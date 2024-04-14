using System.Text;

namespace LethalCompanyInputUtils.SourceGen;

internal class SourceBuilder
{
    private readonly StringBuilder _builder = new();
    private int _depth;
    private bool _usedTabsThisLine;

    public SourceBuilder IncrementDepth()
    {
        _depth++;

        return this;
    }
    
    public SourceBuilder DecrementDepth()
    {
        _depth--;

        if (_depth < 0 )
            _depth = 0;

        return this;
    }

    public SourceBuilder Append(string text)
    {
        if (!_usedTabsThisLine)
        {
            for (int i = 0; i < _depth; i++)
                _builder.Append('\t');

            _usedTabsThisLine = false;
        }
        
        _builder.Append(text);

        return this;
    }

    public SourceBuilder AppendLine(string line)
    {
        Append(line);

        _builder.Append('\n');
        _usedTabsThisLine = false;

        return this;
    }

    public override string ToString()
    {
        return _builder.ToString();
    }
}