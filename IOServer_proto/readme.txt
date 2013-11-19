Using the I/O redirecting capability of .Net Processes, inter-process communication can be achieved with great simplicity in implementation.

The communication mode is described by a master/slave model where AZUSA is the master and all other external processes are slaves.

After connecting to AZUSA and retrieving information on other engines from AZUSA, the engine can establish other forms of connections such as pub/sub with 0MQ.

IOServer_proto is a side project aiming at building a managed message server based on hidden shell and standard I/O redirecting.

A protocol (temporarily named protoNYAN) for information retrieval and engine self-identification will also be implemented.
