// IAnimationState.cs
namespace Title.Animation.States
{
    public interface IAnimationState
    {
        void Enter(AnimatedImageController controller);
        void Exit(AnimatedImageController controller);
        void OnClick(AnimatedImageController controller);
        void OnHoverEnter(AnimatedImageController controller);
        void OnHoverExit(AnimatedImageController controller);
    }
}