import { ThemeProvider } from "@material-ui/core";
import React, { createContext } from "react";
import { SWRConfig } from "swr";
import AzureAdAuthenticator from "./components/AzureAdAuthenticator";
import Tables from "./components/Tables";
import config from "./config";
import theme from "./theme";

//@ts-ignore
export const AlvtimeContext = createContext();

function createAlvtimeFetcher(
  adAuthenticatedFetch: (path: string, options: any) => Promise<Response>
) {
  return async (path: string, paramOptions: any) => {
    const contentTypeHeader = {
      "Content-Type": "application/json",
    };
    paramOptions = paramOptions ? paramOptions : { headers: {} };

    if (paramOptions.body) {
      paramOptions.body = JSON.stringify(paramOptions.body);
    }

    var options = {
      ...paramOptions,
      headers: {
        ...paramOptions.headers,
        ...contentTypeHeader,
      },
    };

    return adAuthenticatedFetch(config.API_HOST + path, options).then((r) =>
      r.json()
    );
  };
}

function App() {
  return (
    <ThemeProvider theme={theme}>
      <AzureAdAuthenticator
        render={(
          adAuthenticatedFetch: (
            path: string,
            options: any
          ) => Promise<Response>
        ) => (
          <AlvtimeContext.Provider
            value={{
              alvtimeFetcher: createAlvtimeFetcher(adAuthenticatedFetch),
            }}
          >
            <SWRConfig
              value={{
                fetcher: createAlvtimeFetcher(adAuthenticatedFetch),
                revalidateOnFocus: false,
              }}
            >
              <Tables />
            </SWRConfig>
          </AlvtimeContext.Provider>
        )}
      ></AzureAdAuthenticator>
    </ThemeProvider>
  );
}

export default App;
