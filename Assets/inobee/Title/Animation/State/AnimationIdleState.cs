// AnimationIdleState.cs
namespace Title.Animation.States
{
    public class AnimationIdleState : IAnimationState
    {
        public void Enter(AnimatedImageController controller)
        {
            controller.StopAnimation();
            controller.ShowThumbnail();
        }

        public void Exit(AnimatedImageController controller)
        {
            controller.HideThumbnail();
        }

        public void OnClick(AnimatedImageController controller)
        {
            controller.ChangeState(new AnimationPlayingState());
            controller.StartPlayTransition();
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