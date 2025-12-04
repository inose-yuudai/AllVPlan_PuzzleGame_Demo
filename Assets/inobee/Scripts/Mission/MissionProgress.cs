namespace EmoteOrchestra.Mission
{
    /// <summary>
    /// ミッションの進行状況
    /// </summary>
    public class MissionProgress
    {
        public MissionData Data { get; private set; }
        public int CurrentValue { get; private set; }
        public bool IsCompleted { get; private set; }

        public float Progress => Data.targetValue > 0 ? 
            (float)CurrentValue / Data.targetValue : 0f;

        public MissionProgress(MissionData data)
        {
            Data = data;
            CurrentValue = 0;
            IsCompleted = false;
        }

        public void AddProgress(int value)
        {
            if (IsCompleted)
                return;

            CurrentValue += value;

            if (CurrentValue >= Data.targetValue)
            {
                IsCompleted = true;
            }
        }

        public void Reset()
        {
            CurrentValue = 0;
            IsCompleted = false;
        }
    }
}