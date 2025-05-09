import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import LoginPage from "./pages/login/page";
import RegisterPage from "./pages/register/page";
import HomePage from "./pages/Home/page";
import ProtectedRoute from "./routes/ProtectedRoute";
import ProfilePage from "./pages/profile/page";
import CreateTutorialPage from "./pages/createTutorial/page";
import TutorialPage from "./pages/tutorial/page";
import AdminPage from "./pages/admin/page";
import TutorialsPage from "./pages/tutorials/page";

export default function App() {
  return(
    <Router>
      <Routes>
        <Route path="/" element={<HomePage/>} />
        <Route path="/login" element={<LoginPage/>} />
        <Route path="/register" element={<RegisterPage/>} />
        <Route path="/tutorials" element={<ProtectedRoute><TutorialsPage/></ProtectedRoute>} />
        <Route path="/profile" element={<ProtectedRoute><ProfilePage/></ProtectedRoute>} />
        <Route path="/createTutorial" element={<ProtectedRoute><CreateTutorialPage/></ProtectedRoute>} />
        <Route path="/tutorial/:id" element={<TutorialPage/>}/>
        <Route path="/edit-tutorial/:id" element={<CreateTutorialPage />} />
        <Route path="/admin" element={<AdminPage />} />
      </Routes>
    </Router>
  )
};
