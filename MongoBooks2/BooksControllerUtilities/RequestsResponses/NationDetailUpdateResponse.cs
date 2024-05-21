using BooksControllerUtilities.DataClasses;

namespace BooksControllerUtilities.RequestsResponses
{
    public class NationDetailUpdateResponse
    {
        public NationDetail UpdateItem { get; set; }

        public int ErrorCode { get; set; }

        public string FailReason { get; set; }

        public string UserId { get; set; }
    }
}
