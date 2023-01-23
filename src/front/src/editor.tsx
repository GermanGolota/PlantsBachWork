import Modal from "react-modal";import "react-draft-wysiwyg/dist/react-draft-wysiwyg.css";
import draftToHtml from "draftjs-to-html";
import { ContentState, convertToRaw, EditorState } from "draft-js";
import { Elm as AddEditInstructionElm } from "./Elm/Pages/AddEditInstruction";
import React from "react";
import { retrieve } from "./Store";
import { Editor } from "react-draft-wysiwyg";
import { useNavigate, useParams } from "react-router-dom";
import htmlToDraft from "html-to-draftjs";

const AddInstructionPage = (props: { isEdit: boolean }) => {
  const [app, setApp] = React.useState<
    AddEditInstructionElm.Pages.AddEditInstruction.App | undefined
  >();
  const [editorVisible, setEditorVisible] = React.useState<boolean>(false);
  const [state, setState] = React.useState<EditorState>(
    EditorState.createEmpty()
  );
  const elmRef = React.useRef(null);
  const navigate = useNavigate();
  const { id } = useParams();

  const elmApp = () => {
    let model: any = retrieve();
    if (props.isEdit) {
      model = {
        ...model,
        id: id,
      };
    }
    model = {
      ...model,
      isEdit: props.isEdit,
    };

    return AddEditInstructionElm.Pages.AddEditInstruction.init({
      node: elmRef.current,
      flags: model,
    });
  };

  // Subscribe to state changes from Elm
  React.useEffect(() => {
    if (app) {
      app.ports.openEditor.subscribe((txt) => {
        let state = convertText(txt);
        setState(state);
        setEditorVisible(true);
      });
      app.ports.navigate.subscribe((location) => {
        navigate(location);
      });
    }
  }, [app, navigate]);

  React.useEffect(() => {
    setApp(elmApp());
  }, []);

  if (editorVisible) {
    return (
      <div>
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
        </div>
        <div>
          <div ref={elmRef}></div>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const convertText = (text: string) => {
  const blocksFromHtml = htmlToDraft(text);
  const { contentBlocks, entityMap } = blocksFromHtml;
  const contentState = ContentState.createFromBlockArray(
    contentBlocks,
    entityMap
  );
  return EditorState.createWithContent(contentState);
};

export default AddInstructionPage;
