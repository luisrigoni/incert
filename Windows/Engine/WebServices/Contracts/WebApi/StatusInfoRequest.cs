﻿using Org.InCommon.InCert.Engine.WebServices.Managers;
using RestSharp;

namespace Org.InCommon.InCert.Engine.WebServices.Contracts.WebApi
{
    class StatusInfoRequest:AbstractStatusInfoContract
    {
        public StatusInfoRequest(IEndpointManager endpointManager) : base(endpointManager)
        {
        }

        protected override IRestRequest GetRequestObject()
        {
            var request = new RestRequest(Method.GET)
            {
                RootElement = "status",
                DateFormat = "ddd MMM d HH:mm:ss yyyy"
            };

            return request;
        }
    }
}
