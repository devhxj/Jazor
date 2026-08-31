/*jazor:clr-member System.Uri.PathAndQuery.get*/
export function getPathAndQuery(instance) {
  return instance.pathname + instance.search;
}
/*jazor:clr-member System.Uri.Port.get*/
export function getPort(instance) {
  let port = instance.port;
  if (port.length !== 0)
    return parseInt(port, 10);
  let protocol = instance.protocol;
  if (protocol === "https:" || protocol === "wss:")
    return 443;
  if (protocol === "http:" || protocol === "ws:")
    return 80;
  if (protocol === "ftp:")
    return 21;
  return -1;
}
