import Modal from "react-modal";import "react-draft-wysiwyg/dist/react-draft-wysiwyg.css";
import draftToHtml from "draftjs-to-html";
import { ContentState, convertToRaw, EditorState } from "draft-js";
import { Elm as AddEditInstructionElm } from "./Elm/Pages/AddEditInstruction";
import React from "react";
import { Editor } from "react-draft-wysiwyg";
import { useParams } from "react-router-dom";
import htmlToDraft from "html-to-draftjs";
import { useElmApp } from "./hooks";

const AddInstructionPage = (props: { isEdit: boolean }) => {
  const [editorVisible, setEditorVisible] = React.useState<boolean>(false);
  const [state, setState] = React.useState<EditorState>(
    EditorState.createEmpty()
  );
  const { id } = useParams();
  const { elmRef, ports } = useElmApp(
    AddEditInstructionElm.Pages.AddEditInstruction.init,
    {
      setFlags: (model) => {
        let result: any = {
          ...model,
          isEdit: props.isEdit,
        };

        if (props.isEdit) {
          result = {
            ...result,
            id: id,
          };
        }

        return result;
      },
      additional: (ports) => {
        ports.openEditor.subscribe((txt) => {
          let state = convertText(txt);
          setState(state);
          setEditorVisible(true);
        });
      },
    }
  );

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
                ports.editorChanged.send(text);
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
