import Modal from "react-modal";import "react-draft-wysiwyg/dist/react-draft-wysiwyg.css";
import draftToHtml from "draftjs-to-html";
import { convertToRaw, EditorState } from "draft-js";
import { Elm as AddInstructionElm } from "./Elm/Pages/AddInstruction";
import React from "react";
import { retrieve } from "./Store";
import { Editor } from "react-draft-wysiwyg";

const AddInstructionPage = () => {
  const [app, setApp] = React.useState<
    AddInstructionElm.Pages.AddInstruction.App | undefined
  >();
  const [editorVisible, setEditorVisible] = React.useState<boolean>(false);
  const [state, setState] = React.useState<EditorState>(
    EditorState.createEmpty()
  );
  const elmRef = React.useRef(null);

  const elmApp = () => {
    let model = retrieve();

    return AddInstructionElm.Pages.AddInstruction.init({
      node: elmRef.current,
      flags: model,
    });
  };

  // Subscribe to state changes from Elm
  React.useEffect(() => {
    app &&
      app.ports.openEditor.subscribe((_) => {
        setEditorVisible(true);
      });
  }, [app]);

  React.useEffect(() => {
    setApp(elmApp());
  }, []);

  if (editorVisible) {
    return (
      <div>
        <Modal
          isOpen={editorVisible}
          onRequestClose={() => setEditorVisible(false)}
          contentLabel="Instruction Text"
        >
          <Editor
            editorState={state}
            onEditorStateChange={(newState) => {
              let text = draftToHtml(
                convertToRaw(newState.getCurrentContent())
              );
              setState(newState);
              app?.ports.editorChanged.send(text);
            }}
          />
        </Modal>
        <div ref={elmRef}></div>
      </div>
    );
  }

  return <div ref={elmRef}></div>;
};

export default AddInstructionPage;
