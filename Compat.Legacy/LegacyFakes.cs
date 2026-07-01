using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace Compat.Legacy
{
    /// <summary>
    /// Minimal in-memory <see cref="HttpContextBase"/> so MVC5 value-provider factories and model
    /// binding run WITHOUT IIS/System.Web hosting. Only the members the binders touch are populated.
    /// </summary>
    internal sealed class FakeHttpContext : HttpContextBase
    {
        private readonly FakeHttpRequest _request;
        private readonly FakeHttpResponse _response = new FakeHttpResponse();
        private readonly IDictionary _items = new Dictionary<object, object>();

        public FakeHttpContext(FakeHttpRequest request) { _request = request; }

        public override HttpRequestBase Request => _request;
        public override HttpResponseBase Response => _response;
        public override IDictionary Items => _items;
        public override IPrincipal User { get; set; }
    }

    internal sealed class FakeHttpRequest : HttpRequestBase
    {
        private readonly string _httpMethod;
        private readonly string _contentType;
        private readonly Stream _inputStream;
        private readonly NameValueCollection _query;
        private readonly NameValueCollection _form;
        private readonly NameValueCollection _headers;

        public FakeHttpRequest(string httpMethod, string contentType, byte[] body,
                               NameValueCollection query, NameValueCollection form, NameValueCollection headers)
        {
            _httpMethod = httpMethod ?? "GET";
            _contentType = contentType ?? string.Empty;
            _inputStream = new MemoryStream(body ?? new byte[0], writable: false);
            _query = query ?? new NameValueCollection();
            _form = form ?? new NameValueCollection();
            _headers = headers ?? new NameValueCollection();
        }

        public override string HttpMethod => _httpMethod;
        public override string ContentType { get => _contentType; set { } }
        public override Stream InputStream => _inputStream;
        public override NameValueCollection QueryString => _query;
        public override NameValueCollection Form => _form;
        public override NameValueCollection Headers => _headers;
        public override Encoding ContentEncoding { get => Encoding.UTF8; set { } }

        public override NameValueCollection Params
        {
            get
            {
                var p = new NameValueCollection();
                p.Add(_query);
                p.Add(_form);
                return p;
            }
        }
    }

    internal sealed class FakeHttpResponse : HttpResponseBase
    {
        private readonly StringWriter _writer = new StringWriter();
        public override int StatusCode { get; set; } = 200;
        public override string ContentType { get; set; } = "application/json";
        public override TextWriter Output => _writer;
        public override void Write(string s) => _writer.Write(s);
    }
}
