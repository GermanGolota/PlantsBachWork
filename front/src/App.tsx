import "./App.css";
import { Elm } from "react-elm-components";
import { Stats } from "elm/Pages/Stats";

function App() {
  return (
    <div className="App">
      <Elm src={Stats} />
    </div>
  );
}

export default App;
