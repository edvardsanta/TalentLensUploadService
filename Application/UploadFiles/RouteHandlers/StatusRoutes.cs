namespace UploadFiles.RouteHandlers
{
    public static class StatusRoutes
    {
        public static void Configure(RouteGroupBuilder statusMap)
        {
            //statusMap.MapGet("/{referenceId}", GetStatus);
        }

        private static object? GetStatusFromReferenceId(string referenceId)
        {
            // Implementation of status retrieval logic
            throw new NotImplementedException();
        }
    }
}
