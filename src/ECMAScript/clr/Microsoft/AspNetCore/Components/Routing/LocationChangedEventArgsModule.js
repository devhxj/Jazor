/*jazor:clr-member Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs.LocationChangedEventArgs(string, bool)*/
export function createLocationChangedEventArgs(location, isNavigationIntercepted) {
  return {
    location: location,
    isNavigationIntercepted: isNavigationIntercepted,
    historyEntryState: window.history.state
  };
}
