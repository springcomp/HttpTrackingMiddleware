HttpTrackingMiddleware
======================

A custom [Owin](http://owin.org/) Middleware to track details of an HTTP request and response.

This is a sample of a custom Owin Middleware, designed to track details and HTTP request and response.
It allows you to record extensive details about an HTTP request to a durable store and return the response
to the caller with a *Tracking Identifier*.

![Tracking HTTP Request and Response](https://cloud.githubusercontent.com/assets/8488398/4556276/5a1dd32c-4ec7-11e4-9174-67cbb251efaa.png "Tracking HTTP Request and Response")

That way, in case of an error, the caller can call your customer support staff to troubleshoot the real cause
of an error.

## Rationale and Explanations

You can find a detailed walkthrough about designing this Middleware in the following post from my Blog:

[http://maximelabelle.wordpress.com/2014/10/08/capturing-rest-api-calls-with-an-owin-middleware/](http://maximelabelle.wordpress.com/2014/10/08/capturing-rest-api-calls-with-an-owin-middleware/)
