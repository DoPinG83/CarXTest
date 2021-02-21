using UnityEngine;

public static class AnimationExtensions
{
    #region Animation by Index

    public static AnimationState GetClipByIndex(this Animation animation, int index)
    {
        int i = 0;
        foreach (AnimationState animationState in animation)
        {
            if (i == index)
                return animationState;
            i++;
        }
        return null;
    }

    public static void PlayClipByIndex(this Animation animation, int index)
    {
        animation.Play(animation.GetClipByIndex(index).name);
    }

    public static void PlayClipByIndex(this Animation animation, int index, PlayMode mode)
    {
        animation.Play(animation.GetClipByIndex(index).name, mode);
    }

    public static bool IsPlayingByIndex(this Animation animation, int index)
    {
        return animation.IsPlaying(animation.GetClipByIndex(index).name);
    }

    #endregion
}
