**AutoWikiBrowser v2** (ABWv2) is, as the name suggests, an updated version of AWB, built from the ground up with .NET Core 8.0 and AvaloniaUI.

Leveraging .NET Core and AvaloniaUI means that AWB can be cross-platform and run on macOS and Linux as well as Windows, which is something that
hasn't previously been possible — whilst PWB is a good alternative, it is overkill for some people, especially those who are not technically inclined.

> Note: this repository is not production ready and is a proof-of-concept; it may never graduate to being production ready. 

### Differences
Most of this repository take inspiration from AutoWikiBrowser; some of the stuff has been copied from there directly, or changed
slightly to make work cross-platform. AWBv2 is meant to be used for any wiki and therefore, a lof of the Wikipedia-specific
stuff has been removed. For example, general fixes that are Wikipedia specific have been removed completely, 
since they add development work to get working cross-platform and are generally only useful for Wikipedia. 

Other Wikipedia specific stuff like checking against an allowed bot list etc. have been removed. There are no plans currently 
to support such stuff, but if someone wishes to add those--or any of the stuff that has been removed--feel free, and it will be merged.

As an aside, direct changes to the repository include the fact that `WikiFunctions` has simply been renamed to `Functions`, and things
will only be implemented if they can truly work cross-platform. Things that only work on Windows will not be implemented as it adds more 
development work to ensure they only run on Windows etc. 
### Licensing
This program is free software, and licensed under the GPLv3; it takes inspiration from the original AWB and uses some code from that repository, with some changes and improvements.
This project would not have been possible without the work of the original authors. The original repository can be found [here](https://sourceforge.net/p/autowikibrowser/code/HEAD/tree/AWB/AWB/).
