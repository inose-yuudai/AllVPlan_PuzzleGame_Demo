// AnimationPlayingState.cs
namespace Title.Animation.States
{
    public class AnimationPlayingState : IAnimationState
    {
        public void Enter(AnimatedImageController controller)
        {
            controller.PlayAnimation();
        }

        public void Exit(AnimatedImageController controller)
        {
            controller.StopAnimation();
        }

        public void OnClick(AnimatedImageController controller)
        {
            // 再生中はクリック無効
        }

        public void OnHoverEnter(AnimatedImageController controller)
        {
            controller.ShowControls();
        }

        public void OnHoverExit(AnimatedImageController controller)
        {
            controller.HideControls();
        }
    }
}