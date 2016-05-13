using System.Collections.Immutable;

namespace ManifestImportExportAPI.Repositories
{
    public enum QueryStatus
    {
        OK, NO_DATA, FAILED_CONNECTION, FAIL
    }


    public interface IQueryStatus
    {
        QueryStatus Status { get; }
    }

    public class RetrieveResult<T> : IQueryStatus
    {
        public T Result { get; private set; }
        public QueryStatus Status { get; private set; }

        public RetrieveResult(T result, QueryStatus status)
        {
            Result = result;
            Status = status;
        }

        public RetrieveResult()
        {
            Status = QueryStatus.NO_DATA;
        }
    }

    public class RetrieveResults<T> : IQueryStatus
    {
        public ImmutableList<T> Results { get; private set; }
        public QueryStatus Status { get; private set; }

        public RetrieveResults(ImmutableList<T> results, QueryStatus status)
        {
            Results = results;
            Status = status;
        }

        public RetrieveResults()
        {
            Results = ImmutableList<T>.Empty;
            Status = QueryStatus.NO_DATA;
        }
    }
}