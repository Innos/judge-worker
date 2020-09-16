namespace OJS.Workers.SubmissionProcessors.SubmissionProcessors
{
    public interface ISubmissionProcessor
    {
        string Name { get; set; }

        void Start();

        void Stop();
    }
}