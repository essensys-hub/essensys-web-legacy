using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Essensys.Repository.DTO;
using System.Web;
using Essensys.Common;

namespace Essensys.Service.Security
{
    public class EssensysAuthorizeAttribute : AuthorizeAttribute
    {
        private const string BasicAuthResponseHeader = "WWW-Authenticate";
        private const string BasicAuthResponseHeaderValue = "Basic";

        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (AuthorizeRequest(actionContext.ControllerContext.Request))
                return;
            this.HandleUnauthorizedRequest(actionContext);
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = CreateUnauthorizedResponse(actionContext
                .ControllerContext.Request);
        }
        private HttpResponseMessage CreateUnauthorizedResponse(HttpRequestMessage request)
        {
            var result = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Unauthorized,
                RequestMessage = request
            };
            //we need to include WWW-Authenticate header in our response,
            //so our client knows we are using HTTP authentication
            result.Headers.Add(BasicAuthResponseHeader, BasicAuthResponseHeaderValue);
            return result;
        }

        private bool AuthorizeRequest(HttpRequestMessage request)
        {
            AuthenticationHeaderValue authValue = request.Headers.Authorization;
            if (authValue == null || String.IsNullOrWhiteSpace(authValue.Parameter)
                || String.IsNullOrWhiteSpace(authValue.Scheme)
                || authValue.Scheme != BasicAuthResponseHeaderValue)
            {
                return false;
            }
            string[] parsedHeader = ParseAuthorizationHeader(authValue.Parameter);
            if (parsedHeader == null)
            {
                return false;
            }
            return TestAuthorization(parsedHeader);
        }

        private bool TestAuthorization(string[] parsedHeader)
        {
            string cryptedcode = parsedHeader[0] + parsedHeader[1];
            EsMachine m = new UserService().ValidateAPIAccess(cryptedcode);

            if (m != null)
            {
                HttpContext.Current.Session["Machine"] = m;
                return true;
            }
            else
                return false;
        }

        private string[] ParseAuthorizationHeader(string authHeader)
        {
            string[] credentials = Encoding.UTF8.GetString(Convert
                                                            .FromBase64String(authHeader))
                                                            .Split(
                                                            new[] { ':' });
            if (credentials.Length != 2 || string.IsNullOrEmpty(credentials[0])
                || string.IsNullOrEmpty(credentials[1])) return null;
            return credentials;
        }
    }
}
