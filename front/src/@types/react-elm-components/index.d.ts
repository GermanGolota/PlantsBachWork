declare module 'react-elm-components'
{
  import * as React from "react";

  export interface ElmProps {
    src: any
  }

  declare class Elm extends React.Component<ElmProps, any> { }
}

