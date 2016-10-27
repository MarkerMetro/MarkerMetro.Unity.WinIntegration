
using Facebook;

namespace MarkerMetro.Unity.WinIntegration.Facebook
{
    public delegate void FacebookDelegate(FBResult result);
    public delegate void InitDelegate();
    public delegate void HideUnityDelegate(bool hideUnity);

    public class FBResult
    {
        public string Error { get; set; }
        public string Text { get; set; }
#if WINDOWS_UWP
        public object Object { get; set; }
#else
        public JsonObject Json { get; set; }
#endif

#if WINDOWS_UWP
        public FBResult () { }

        public FBResult (global::Facebook.FBResult fbResult)
        {
            Text = fbResult.Succeeded ? "Success" : "Fail";
            Error = fbResult.Succeeded ? null : fbResult.ErrorInfo.Message;
            Object = fbResult.Object;
        }
#endif
    }

    public enum HttpMethod
    {
        GET,
        POST,
        DELETE
    }

}
