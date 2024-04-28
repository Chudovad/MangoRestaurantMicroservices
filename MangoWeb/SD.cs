namespace Mango.Web
{
    public static class SD
    {
        public static string ProductAPIBase { get; set; }
        public static string AuthAPIBase { get; set; }
        public const string RoleAdmin = "ADMIN";
        public const string RoleCustomer = "CUSTOMER";
        public const string TokenCookie = "JWTToken";
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        public enum ContentType
        {
            Json,
            MultipartFormData,
        }
    }
}
