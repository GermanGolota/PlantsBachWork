import React, { useEffect } from "react";import ReactDOM from "react-dom";
import {
  Route,
  BrowserRouter,
  Routes,
  useParams,
  useNavigate,
} from "react-router-dom";
import { Elm as StatsElm } from "./Elm/Pages/Stats";
import { Elm as LoginElm } from "./Elm/Pages/Login";
import { Elm as SearchElm } from "./Elm/Pages/Search";
import { Elm as HistoryElm } from "./Elm/Pages/History";
import { Elm as PlantElm } from "./Elm/Pages/Plant";
import { Elm as NotPostedElm } from "./Elm/Pages/NotPosted";
import { Elm as PostPlantElm } from "./Elm/Pages/PostPlant";
import { Elm as AddEditPlantElm } from "./Elm/Pages/AddEditPlant";
import { Elm as OrdersElm } from "./Elm/Pages/Orders";
import { Elm as UsersElm } from "./Elm/Pages/Users";
import { Elm as InstructionElm } from "./Elm/Pages/Instruction";
import { Elm as AddUserElm } from "./Elm/Pages/AddUser";
import { Elm as ProfileElm } from "./Elm/Pages/Profile";
import { Elm as SearchInstructionsElm } from "./Elm/Pages/SearchInstructions";
import "./assets/tree.svg";
import "bootstrap/dist/css/bootstrap.min.css";
import "@fortawesome/fontawesome-free/css/all.min.css";
import "./main.css";
import { AuthResponse, retrieve, store } from "./Store";
import AddInstructionPage from "./editor";
import { useElmApp } from "./hooks";

const HistoryPage = () => {
  const { name, id } = useParams();

  const { elmRef } = useElmApp(HistoryElm.Pages.History.init, {
    setFlags: (model) => {
      return {
        ...model,
        id,
        name,
      };
    },
    additional: (flags) => {
      flags.resizeAggregates.subscribe(() => {
        const accordions = document.getElementsByClassName("accordion");
        for (let index = 0; index < accordions.length; index++) {
          const accordion = accordions[index];
          if (accordion) {
            for (let index = 0; index < accordion.childNodes.length; index++) {
              const cards = accordion.childNodes[index];
              for (let index2 = 0; index2 < cards.childNodes.length; index2++) {
                const body = cards.childNodes[index2];
                if (body instanceof HTMLElement && body.id) {
                  console.log("child", body);
                  if (body.style.height == "0px") {
                    //element.style.removeProperty("height");
                    //element.style.height = "0%";
                  } else {
                    body.style.removeProperty("height");
                    body.style.height = "100%";
                  }
                }
              }
            }
          }
        }
      });
    },
  });

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const SearchPage = () => {
  const { name, id } = useParams();
  const { elmRef } = useElmApp(SearchElm.Pages.Search.init, {
    setFlags: (model) => {
      return { ...model, name, id };
    },
  });

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const InstructionPage = () => {
  const { id } = useParams();

  const { elmRef } = useElmApp<InstructionElm.Pages.Instruction.App>(
    InstructionElm.Pages.Instruction.init,
    {
      setFlags: (model) => {
        return {
          ...model,
          id: id,
        };
      },
    }
  );

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const SearchInstructionsPage = () => {
  const { elmRef } =
    useElmApp<SearchInstructionsElm.Pages.SearchInstructions.App>(
      SearchInstructionsElm.Pages.SearchInstructions.init,
      {}
    );

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const ProfilePage = () => {
  const { elmRef } = useElmApp<ProfileElm.Pages.Profile.App>(
    ProfileElm.Pages.Profile.init,
    {}
  );

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const UsersPage = () => {
  const { elmRef } = useElmApp<UsersElm.Pages.Users.App>(
    UsersElm.Pages.Users.init,
    {}
  );

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const AddUserPage = () => {
  const { elmRef } = useElmApp<AddUserElm.Pages.AddUser.App>(
    AddUserElm.Pages.AddUser.init,
    {}
  );

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const NotPostedPage = () => {
  const { elmRef } = useElmApp<NotPostedElm.Pages.NotPosted.App>(
    NotPostedElm.Pages.NotPosted.init,
    {}
  );

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const AddEditPage = (props: { isEdit: boolean }) => {
  const { plantId } = useParams();

  const { elmRef } = useElmApp<AddEditPlantElm.Pages.AddEditPlant.App>(
    AddEditPlantElm.Pages.AddEditPlant.init,
    {
      setFlags: (model) => {
        return {
          ...model,
          plantId: plantId,
          isEdit: props.isEdit,
        };
      },
    }
  );

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const OrdersPage = (props: { isEmployee: boolean }) => {
  const { elmRef } = useElmApp<OrdersElm.Pages.Orders.App>(
    OrdersElm.Pages.Orders.init,
    {
      setFlags: (model) => {
        let isEmployeeFinal = props.isEmployee;

        if (model.roles.length == 1 && model.roles[0] == "Consumer") {
          isEmployeeFinal = false;
        } else {
          if (model.roles.some((a) => a == "Consumer") == false) {
            isEmployeeFinal = true;
          }
        }

        return {
          ...model,
          isEmployee: isEmployeeFinal,
        };
      },
    }
  );

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const PlantPage = (props: { isOrder: boolean }) => {
  const { plantId } = useParams();

  const { elmRef } = useElmApp<PlantElm.Pages.Plant.App>(
    PlantElm.Pages.Plant.init,
    {
      setFlags: (model) => {
        return {
          ...model,
          plantId: plantId,
          isOrder: props.isOrder,
        };
      },
    }
  );

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const PostPlantPage = () => {
  const { plantId } = useParams();

  const { elmRef } = useElmApp<PostPlantElm.Pages.PostPlant.App>(
    PostPlantElm.Pages.PostPlant.init,
    {
      setFlags: (model) => {
        return {
          ...model,
          plantId: plantId,
        };
      },
    }
  );

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const StatsPage = () => {
  const { elmRef } = useElmApp<StatsElm.Pages.Stats.App>(
    StatsElm.Pages.Stats.init,
    {}
  );

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const LoginPage = (props: { isNew: boolean }) => {
  const { elmRef } = useElmApp<LoginElm.Pages.Login.App>(
    LoginElm.Pages.Login.init,
    {
      onSetApp: () => {
        if (retrieve()) {
          if (props.isNew) {
            localStorage.clear();
          } else {
            window.location.replace("/search");
          }
        }
      },
      setFlags: (_) => null,
      additional: (ports) => {
        ports.notifyLoggedIn.subscribe((userModel) => {
          let model = userModel as AuthResponse;
          store(model);
          window.location.replace("/search");
        });
      },
    }
  );

  return (
    <div>
      <div ref={elmRef}></div>
    </div>
  );
};

const NavigateWrapper = () => {
  const navigate = useNavigate();
  const { location } = useParams();

  useEffect(() => {
    if (location) {
      if (location == "-1") {
        navigate(-2);
      } else {
        navigate(decodeURIComponent(location), {
          replace: true,
        });
      }
    }
  }, []);
  return <div></div>;
};

const NotFound = () => {
  return (
    <div>
      There is nothing at this url. Maybe you wanted to{" "}
      <a href="/login">log in</a>?
    </div>
  );
};

const App = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/wrapper/:location" element={<NavigateWrapper />}></Route>
        <Route path="/login" element={<LoginPage isNew={false} />} />
        <Route path="/login/new" element={<LoginPage isNew={true} />} />
        <Route path="/stats" element={<StatsPage />} />
        <Route path="/search" element={<SearchPage />} />
        <Route path="/notPosted" element={<NotPostedPage />} />
        <Route path="/plant/:plantId" element={<PlantPage isOrder={false} />} />
        <Route
          path="/plant/:plantId/order"
          element={<PlantPage isOrder={true} />}
        />
        <Route path="/notPosted/:plantId/post" element={<PostPlantPage />} />
        <Route path="/notPosted/add" element={<AddEditPage isEdit={false} />} />
        <Route
          path="/notPosted/:plantId/edit"
          element={<AddEditPage isEdit={true} />}
        />
        <Route path="/orders" element={<OrdersPage isEmployee={false} />} />
        <Route
          path="/orders/employee"
          element={<OrdersPage isEmployee={true} />}
        />
        <Route path="/user" element={<UsersPage />} />
        <Route path="/user/add" element={<AddUserPage />} />
        <Route path="/instructions" element={<SearchInstructionsPage />} />
        <Route
          path="/instructions/add"
          element={<AddInstructionPage isEdit={false} />}
        />
        <Route
          path="/instructions/:id/edit"
          element={<AddInstructionPage isEdit={true} />}
        />
        <Route path="/instructions/:id" element={<InstructionPage />} />
        <Route path="/profile" element={<ProfilePage />} />
        <Route path="/history/:name/:id" element={<HistoryPage />} />
        <Route path="*" element={<NotFound />} />
      </Routes>
    </BrowserRouter>
  );
};

ReactDOM.render(<App />, document.querySelector("#root"));
