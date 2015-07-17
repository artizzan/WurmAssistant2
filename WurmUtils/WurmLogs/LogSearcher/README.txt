
Please note that LogSearcher is very old code, that was not very well designed at first.
It was running slow, so it was decided to use threading, but that implementation 
could've been considered far from perfect.

While refactoring the code towards standalone library, it was necessary to make it's usage
thread-safe. It was done by refactoring LogSearchManager to use internal concurrent queue,
that operates on Tasks, one at a time. Access to this system from outside this library
is provided via API of async methods.

Existing search UI was maintained in mostly unchanged form, which means that search results
are not passed to it using async-await pattern, but instead Form is invoked upon 
search completion. Due to that, Form does NOT need to run on main synchronization context,
although with current implementation it does. This is important, because much of heavy code
runs after data exits threaded task - this is the code responsible for highlighting search
results and building the result list in general. UI responsiveness is somewhat maintained using
Application.DoEvents() method. Again, this is important in case of any event, that could
affect this processing under DoEvents and possibly even cause app to crash.

Internally queue handling is being done via UI Form timer events, as it was done from very 
beginning. There was no reason to modify that behaviour, even if it causes extra tight 
coupling, due to possibly introducing new bugs and async-await issues.

For anyone reading this and interested in improving this code, it is advised to maintain
current code structure and be extra careful about making any major modifications.