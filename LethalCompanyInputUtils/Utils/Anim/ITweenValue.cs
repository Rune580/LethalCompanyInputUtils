namespace LethalCompanyInputUtils.Utils.Anim;

public interface ITweenValue
{
    void TweenValue(float percentage);

    bool IgnoreTimeScale { get; }
    
    float Duration { get; }

    bool ValidTarget();
}