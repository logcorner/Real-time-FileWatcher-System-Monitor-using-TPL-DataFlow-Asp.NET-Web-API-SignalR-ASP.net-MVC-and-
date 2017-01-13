namespace ApiSignalrAngularHub.Controllers
{
    using ApiSignalrAngularHub.Hubs;
    using System.Net;
    using System.Net.Http;

    using ApiSignalrAngularModel;

    public class MonitorController : SignalRBase<monitorHub>
    {
        [System.Web.Http.Route("api/Processor")]
        public HttpResponseMessage PostProcessor(Processor item)
        {
            if (item == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            // notify all connected clients
            Hub.Clients.All.LoadBalance(item);

            // return the item inside of a 201 response
            return Request.CreateResponse(HttpStatusCode.Created, item);
        }

        [System.Web.Http.Route("api/FileOrderEntity")]
        public HttpResponseMessage PostProcessor(FileOrderEntity item)
        {
            if (item == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            // notify all connected clients
            Hub.Clients.All.TransformFileToFileOrderEntity(item);

            // return the item inside of a 201 response
            return Request.CreateResponse(HttpStatusCode.Created, item);
        }
    }
}