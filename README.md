# roddbwpf

My take on the awesome Realms of Despair item db shared with me by Threndor. I've made no changes to the itemdb file and included it in this repo
so that everything you need is present to run.

## Pre-built binaries

There is a github action on push/PR that generates artifacts. Navigate to the Actions tab and find the latest build with a green checkmark then look
at the bottom under 'Artifacts'. There you will find a ~1.5MB zip file that has everything you will need.

## Building from source

This is built using .NET 7 (you will need to install the .NET 7 SDK).
Then you can just `dotnet build` and it should work. (You may need to `dotnet restore` first.)

## Future development
* Integration with mud clients for item collection
* Improved searching capabilities (e.g. str>0)
* Adding area info to db schema