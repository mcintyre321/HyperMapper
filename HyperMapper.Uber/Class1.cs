using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace HyperMapper.Uber
{
    public interface IData
    {
        /// <summary>
        /// The document-wide unique identifier for this element. The value of id must begin with a letter ([A-Za-z]) and may be followed by any number of letters, digits ([0-9]), hyphens ("-"), underscores ("_"), colons (":"), and periods ("."). If the id property is present, it SHOULD be treated as an in-document reference as described in section 3.5 of [RFC3986].
        /// </summary>
        string id { get; }
        /// <summary>
        /// A document-wide non-unique identifier for this element. The value of name must begin with a letter ([A-Za-z]) and may be followed by any number of letters, digits ([0-9]), hyphens ("-"), underscores ("_"), colons (":"), and periods ("."). If the name property is present it MAY be used as a variable in the UBER model property as described in [RFC6570].
        /// </summary>
        string name { get; }

        /// <summary>
        /// Contains a list of link relation values. These values SHOULD conform to the guidance provided in [RFC5988]. In the XML variant the list of link relation values appears as a space-separated list. In the JSON variant the list of link relation values appears as an array.
        /// In UBER documents, the rel property is used to supply domain-related semantic information for the associated data element. This applies equally to data elements used as hypermedia links (e.g. those with a valid url property) and data elements used to carry simple values (e.g. strings and numbers).
        /// </summary>
        IEnumerable<string> rel { get; }

        /// <summary>
        ///Contains a string that represents the caption for the value property of the associated data element. The data element MAY contain this property. If it exists and contains a valid value, applications MAY use it when rendering the documents.
        /// </summary>
        string label { get; }

        /// <summary>
        /// A resolvable URL associated with this element. If the templated property is set to true, the value of the url property SHOULD be treated as a URI Template per [RFC6570]. If the templated property is set of false, contains an unknown value, or is missing, the value of the url property SHOULD be treated as a URL per [RFC3986].
        /// </summary>
        Uri url { get; }


        /// <summary>
        /// If set to true the value of the url property SHOULD be treated as a URI Template per [RFC6570]. The default value is false.
        /// </summary>
        bool templated { get; }


        UberAction action
        /*

action
The network request verb associated with this element. The list of valid values for this element are:

append : An unsafe, non-idempotent request to add a new item (e.g. HTTP.POST [RFC2616])

partial : An unsafe, non-idempotent request to modify parts of an existing item (e.g. HTTP.PATCH [RFC5789])

read : A safe, idempotent request (e.g. HTTP.GET [RFC2616])

remove : An unsafe, idempotent request to delete an existing item (e.g. HTTP.DELETE [RFC2616])

replace : An unsafe, idempotent request to replace an existing item (e.g. HTTP.PUT [RFC2616])

When the <data> element has a url property but no action property, it SHOULD be assumed the action property is set to read. Any unknown value MUST be treated as read.

transclude
Indicates whether the content that is returned from the URL should be embedded within the currently loaded document (transclude="true") or treated as a navigation to a new document (transclude="false"). If no transclude property exists, then the value of transclude SHOULD be assumed to be false (e.g. as a navigation). Any unsupported or unknown transclude value MUST be treated as a navigation.

model
Contains an [RFC6570]-compliant string to be used to construct message bodies. Variables in UBER model strings SHOULD be resolved using the values from name properties, but MAY come from any source available to the client application.

sending
Contains one or more media type identifiers for use when sending request bodies. One of the supplied identifiers SHOULD be selected as a guide when formatting the request body. For HTTP implementations, the selected identifier SHOULD be used as the value for the Content-Type header. If this property is missing the setting should be assumed to be application/x-www-form-urlencoded as described in [RFC1867].

In the XML variant the list of media-type identifiers appears as a space-separated list. In the JSON variant the list of media-type identifiers appears as an array.

accepting
Contains one or more media type identifiers to expect when receiving request bodies. The contents of this property SHOULD indicate the formats in which the server is able to return a response body. For HTTP implementations the contents of this property SHOULD be used as the value for the Accept header. If this property is missing, the setting should be assumed to be set to the same value as that of the currently loaded representation (application/vnd.uber+xml or application/vnd.uber+json).

In the XML variant the list of media-type identifiers appears as a space-separated list. In the JSON variant the list of media-type identifiers appears as an array.

value
In the XML variant of the UBER message format, inner text of the <data> element contains the value associated with that element.

In the JSON variant there is a value property that contains the associated value. Note that the content of this field MUST NOT be a JSON object or array and MUST be one of the following scalar values (listed in Section 2.1 of [RFC4627]):

number

string

false

true

null

For both the XML and JSON variants, it is the responsibility of the document author to make sure the contents of the value property are properly escaped as needed (per Section 2.4 of [REC-XML] and Section 2.5 of [RFC4627]).
         */
    }

    public class Uber
    {
        public string version { get; set; }
        public List<Data> data { get; set; }
    }

    public class RootObject
    {
        public Uber uber { get; set; }
    }
}
