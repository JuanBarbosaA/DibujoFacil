import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Navbar from "./Components/Navbar";
import LoginPage from "./pages/login/page";
import RegisterPage from "./pages/register/page";
import DashboardPage from "./pages/dashboard/page";
import HomePage from "./pages/Home/page";
import ProtectedRoute from "./routes/ProtectedRoute";

export default function App() {
  return(
    <Router>
      <Navbar/>
      <Routes>
        <Route path="/" element={<HomePage/>} />
        <Route path="/login" element={<LoginPage/>} />
        <Route path="/register" element={<RegisterPage/>} />
        <Route path="/dashboard" element={<ProtectedRoute><DashboardPage/></ProtectedRoute>} />
      </Routes>
    </Router>
  )
};
