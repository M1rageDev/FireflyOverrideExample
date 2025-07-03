# Firefly Override Example

This repo contains an example usage of the Firefly API. It showcases all of its features and has code comments.

## Building
KSP Build Tools is setup here, so the only thing you need to do is point it to a KSP install. [Here](https://kspbuildtools.readthedocs.io/en/stable/msbuild/getting-started.html#locating-your-ksp-install) is all the info you need.

You also have to specify the KSPRoot parameter (like shown in the link above) in your .csproj.user file so that it can find the FireflyAPI dll.

FireflyAPI needs to be in the KSP install for this to work.