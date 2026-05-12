const jsonHeaders = Object.freeze({
  Accept: "application/json"
});

export async function getCatalog(signal) {
  return await getJson("/api/playground/examples", signal);
}

export async function getExampleDetail(id, signal) {
  if (typeof id !== "string" || id.trim().length === 0) {
    throw new Error("Playground detail id is required.");
  }

  return await getJson(`/api/playground/examples/${encodeURIComponent(id)}`, signal);
}

async function getJson(url, signal) {
  const response = await fetch(url, {
    method: "GET",
    headers: jsonHeaders,
    cache: "no-store",
    credentials: "same-origin",
    signal
  });

  if (!response.ok) {
    const body = await response.text().catch(() => "");
    throw new Error(`Request failed (${response.status}) for '${url}'. ${body}`.trim());
  }

  return await response.json();
}
