# DVB Transport Stream Tools

## Introduction

This project is the revival of my ancient DVB transport stream multiplexer.

It is somewhat unique in the open source community since it was written specifically to support CableLabs compliant streams as well as supporting
subtitles. While the subtitling code is long lost as it was considered proprietary by my former employer, the transport stream multiplexer itself
survived.

I received a mail recently from a user who had a really odd request. He needed to pass Manzanita transport stream analysis requirements for
MPEG-1 elementary streams which is a use-case I never considered. He was kind enough to send me some sample streams to test with and I was more than
a little surprised that with almost no alteration to the code, I was able to update the code to support MPEG-1. The only real difference I encountered
was that the MPEG-1 files contained user data in the headers which I hadn't been handling up to this point.

## Usage

I've update the project to .NET 4.72 and Visual Studio 2019. So, pretty much everything should run with little work.

I am considering updating the coding style which is... well consistent but it predates static code analysis in Visual Studio, so my naming is a bit ugly.

I also made use of threads to run the multiplexer in the background. I don't see the value in this any longer, so I'm considering removing the thread which
simply isn't needed.

Also, making a command line version which uses a JSON or YAML file would be logical. This way it could run on .NET Core instead. This would support Linux and Mac.
