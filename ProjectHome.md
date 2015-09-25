# Briefly #

Flatiron is a templating framework. It provides some simple syntax to switch between "code" (currently Ruby is supported via Microsoft's DLR/IronRuby) and "literal", along with some include/parenting functionality that is accidentally similar to (and perhaps more flexible than) that of Rails. Integrates with the [Kayak framework](http://code.google.com/p/kayak).

# Get started #

No binary downloads available yet as the design is not quite final. Feel free to poke around the source; you'll need the DLR and IronRuby DLLs to compile it. These can be found in the download 'flatiron-deps.zip'. Note that these DLR binaries leak memory and will likely cause long-running processes to fail.