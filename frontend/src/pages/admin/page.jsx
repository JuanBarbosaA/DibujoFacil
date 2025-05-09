import React, { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import Logout from '../../Components/logout';

export default function AdminPage() {
  const [users, setUsers] = useState([]);
  const [editingUser, setEditingUser] = useState(null);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [tutorials, setTutorials] = useState([]);
  const [activeTab, setActiveTab] = useState('users');
  const [roles, setRoles] = useState([]);
  const [editingTutorial, setEditingTutorial] = useState(null);
const [categories, setCategories] = useState([]);
const [tutorialFormData, setTutorialFormData] = useState({
  title: '',
  status: 'published',
  difficulty: 'beginner',
  categoryIds: []
});
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    points: 0,
    status: 'active',
    roleId: 2
  });
  const [createFormData, setCreateFormData] = useState({
    name: '',
    email: '',
    password: '',
    roleId: 2,
    points: 0,
    status: 'active',
    avatarUrl: ''
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const [contentToDelete, setContentToDelete] = useState([]);
  const [newImages, setNewImages] = useState([]);
  const [imagePreviews, setImagePreviews] = useState([]);

  const fetchData = useCallback(async () => {
    const token = localStorage.getItem('token');
    try {
      const [usersRes, rolesRes, tutorialsRes, categoriesRes] = await Promise.all([
        fetch('http://localhost:5054/api/admin/users', {
          headers: { 'Authorization': `Bearer ${token}` }
        }),
        fetch('http://localhost:5054/api/admin/roles', {
          headers: { 'Authorization': `Bearer ${token}` }
        }),
        fetch('http://localhost:5054/api/admin/tutorials', {
          headers: { 'Authorization': `Bearer ${token}` }
        }),
        fetch('http://localhost:5054/api/admin/categories', {
            headers: { 'Authorization': `Bearer ${token}` }
          })
      ]);

      if (usersRes.status === 403 || rolesRes.status === 403 || tutorialsRes.status === 403) {
        navigate('/');
        return;
      }

      if (!usersRes.ok) throw new Error('Error cargando usuarios');
      if (!rolesRes.ok) throw new Error('Error cargando roles');
      if (!tutorialsRes.ok) throw new Error('Error cargando tutoriales');

      const usersData = await usersRes.json();
      const tutorialsData = await tutorialsRes.json();
      const rolesData = await rolesRes.json();
      const categoriesData = await categoriesRes.json();
      setCategories(categoriesData);
      setUsers(usersData);
      setRoles(rolesData);
      setTutorials(tutorialsData.map(t => ({
        ...t,
        id: Number(t.id), 
        categoryIds: t.categories.map(c => Number(categories.find(cat => cat.name === c))?.id)
      })));
      setError('');
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [navigate]);

  useEffect(() => {
    setLoading(true);
    fetchData();
  }, [fetchData]);

  useEffect(() => {
    if (editingUser) {
      setFormData({
        name: editingUser.name,
        email: editingUser.email,
        points: editingUser.points,
        status: editingUser.status,
        roleId: editingUser.roleId
      });
    }
  }, [editingUser]);

  const handleImageUpload = (e) => {
    const files = Array.from(e.target.files);
    setNewImages(files);
    
    const previews = files.map(file => ({
      id: URL.createObjectURL(file),
      type: file.type,
      contentBase64: URL.createObjectURL(file),
      isNew: true
    }));
    
    setImagePreviews([...imagePreviews, ...previews]);
  };

  const handleDeleteImage = (contentId) => {
    if (typeof contentId === 'string') {
      setImagePreviews(prev => prev.filter(img => img.id !== contentId));
      setNewImages(prev => prev.filter((_, i) => URL.createObjectURL(prev[i]) !== contentId));
    } else {
      setContentToDelete(prev => [...prev, contentId]);
      setImagePreviews(prev => prev.filter(img => img.id !== contentId));
    }
  };


  const handleSubmitTutorial = async (e) => {
    e.preventDefault();
    const token = localStorage.getItem('token');
    
    try {
      // Actualizar metadatos primero
      const metadataResponse = await fetch(`http://localhost:5054/api/admin/tutorials/${editingTutorial.id}`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          title: tutorialFormData.title,
          status: tutorialFormData.status,
          difficulty: tutorialFormData.difficulty,
          categoryIds: tutorialFormData.categoryIds
        })
      });

      if (!metadataResponse.ok) throw new Error('Error actualizando metadatos');

      // Actualizar imágenes
      const formData = new FormData();
      if (contentToDelete.length > 0) {
        formData.append('ContentIdsToDelete', JSON.stringify(contentToDelete));
      }
      
      if (newImages.length > 0) {
        newImages.forEach((file) => {
          formData.append(`NewImages`, file);
        });
      }

      const contentResponse = await fetch(`http://localhost:5054/api/admin/tutorials/${editingTutorial.id}/contents`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`
        },
        body: formData
      });

      if (!contentResponse.ok) throw new Error('Error actualizando imágenes');

      // Actualizar estado local
      const updatedTutorial = await metadataResponse.json();
      setTutorials(prev => prev.map(t => 
        t.id === updatedTutorial.id ? { 
          ...t, 
          ...updatedTutorial,
          lastImage: imagePreviews[imagePreviews.length - 1] 
        } : t
      ));
      
      setEditingTutorial(null);
      setError('');
    } catch (err) {
      setError(err.message);
    }
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    const token = localStorage.getItem('token');
    try {
      const response = await fetch('http://localhost:5054/api/admin/users', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(createFormData)
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Error al crear usuario');
      }

      await fetchData();
      setShowCreateModal(false);
      setCreateFormData({
        name: '',
        email: '',
        password: '',
        roleId: 2,
        points: 0,
        status: 'active',
        avatarUrl: ''
      });
      setError('');
    } catch (err) {
      setError(err.message);
    }
  };

  const handleDeleteTutorial = async (tutorialId) => {
    if (!window.confirm('¿Estás seguro de eliminar este tutorial? Esta acción es irreversible')) return;
  
    const token = localStorage.getItem('token');
    try {
      const response = await fetch(`http://localhost:5054/api/admin/tutorials/${tutorialId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
  
      // Verificar si la respuesta está vacía (204 No Content)
      if (response.status === 204) {
        setTutorials(prev => prev.filter(t => t.id !== tutorialId));
        return;
      }
  
      const result = await response.json();
      
      if (!response.ok) {
        throw new Error(result.message || 'Error al eliminar tutorial');
      }
  
      setTutorials(prev => prev.filter(t => t.id !== tutorialId));
      setError('');
    } catch (err) {
      setError(err.message || 'Error al eliminar el tutorial');
    }
  };

  const handleDelete = async (userId) => {
    if (!window.confirm('¿Estás seguro de eliminar este usuario?')) return;

    const token = localStorage.getItem('token');
    try {
      const response = await fetch(`http://localhost:5054/api/admin/users/${userId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Error al eliminar usuario');
      }

      setUsers(prev => prev.filter(user => user.id !== userId));
      setError('');
    } catch (err) {
      setError(err.message);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const token = localStorage.getItem('token');
    try {
      const response = await fetch(`http://localhost:5054/api/admin/users/${editingUser.id}`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Error al actualizar usuario');
      }

      const updatedUser = await response.json();
      setUsers(prev => prev.map(user => 
        user.id === updatedUser.id ? { ...user, ...updatedUser } : user
      ));
      setEditingUser(null);
      setError('');
    } catch (err) {
      setError(err.message);
    }
  };

  if (loading) return <div className="text-center mt-8">Cargando...</div>;
  if (error) return <div className="text-red-500 text-center mt-8">{error}</div>;

  return (
    <div className="max-w-7xl mx-auto p-6 bg-gray-50 min-h-screen">
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-2xl font-bold text-gray-800">Panel de Administración</h1>
        <Logout className="text-red-600 hover:text-red-800 transition-colors" />
      </div>

      <div className="flex mb-6 border-b border-gray-200">
        <button
          onClick={() => setActiveTab('users')}
          className={`px-4 py-3 font-medium ${
            activeTab === 'users' 
              ? 'border-b-2 border-primary-500 text-primary-600' 
              : 'text-gray-500 hover:text-gray-700'
          } transition-colors`}
        >
          Usuarios ({users.length})
        </button>
        <button
          onClick={() => setActiveTab('tutorials')}
          className={`px-4 py-3 ml-4 font-medium ${
            activeTab === 'tutorials' 
              ? 'border-b-2 border-primary-500 text-primary-600' 
              : 'text-gray-500 hover:text-gray-700'
          } transition-colors`}
        >
          Tutoriales ({tutorials.length})
        </button>
      </div>

      {activeTab === 'users' && (
        <>
          <div className="flex gap-3 mb-6">
            <button
              onClick={fetchData}
              className="px-4 py-2 bg-white text-gray-600 rounded-lg border border-gray-200 hover:bg-gray-50 flex items-center gap-2 shadow-sm"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
              </svg>
              Actualizar
            </button>
            <button
              onClick={() => setShowCreateModal(true)}
              className="px-4 py-2 bg-primary-500 text-white rounded-lg hover:bg-primary-600 transition-colors shadow-sm"
            >
              + Nuevo Usuario
            </button>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  {["ID", "Nombre", "Email", "Registro", "Estado", "Rol", "Puntos", "Tutoriales", "Acciones"].map((header) => (
                    <th key={header} className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">
                      {header}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {users.map(user => (
                  <tr key={user.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4 text-sm text-gray-500 font-mono">{user.id}</td>
                    <td className="px-6 py-4 text-sm font-medium text-gray-900">{user.name}</td>
                    <td className="px-6 py-4 text-sm text-gray-500">{user.email}</td>
                    <td className="px-6 py-4 text-sm text-gray-500">
                      {new Date(user.registrationDate).toLocaleDateString('es-ES', {
                        day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit'
                      })}
                    </td>
                    <td className="px-6 py-4">
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        user.status === 'active' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                      }`}>
                        {user.status === 'active' ? 'Activo' : 'Inactivo'}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">{user.role}</td>
                    <td className="px-6 py-4">
                      <span className="inline-flex items-center px-3 py-1 rounded-full bg-purple-100 text-purple-800 text-sm">
                        {user.points}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <span className="inline-flex items-center px-3 py-1 rounded-full bg-blue-100 text-blue-800 text-sm">
                        {user.tutorialsCount}
                      </span>
                    </td>
                    <td className="px-6 py-4 space-x-3">
                      <button onClick={() => setEditingUser(user)} className="text-primary-600 hover:text-primary-800 font-medium">
                        Editar
                      </button>
                      <button onClick={() => handleDelete(user.id)} className="text-red-600 hover:text-red-800 font-medium">
                        Eliminar
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {activeTab === 'tutorials' && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                {["ID", "Título", "Autor", "Categorías", "Estado", "Contenidos", "Comentarios", "Rating", "Acciones"].map((header) => (
                  <th key={header} className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">
                    {header}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {tutorials.map(tutorial => (
                <tr key={tutorial.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-6 py-4 text-sm text-gray-500 font-mono">{tutorial.id}</td>
                  <td className="px-6 py-4 text-sm font-medium text-gray-900">
                    <div className="flex items-center">
                      {tutorial.lastImage && (
                        <img 
                          src={`data:${tutorial.lastImage.type};base64,${tutorial.lastImage.contentBase64}`}
                          alt="Miniatura"
                          className="w-12 h-12 object-cover rounded mr-3"
                        />
                      )}
                      {tutorial.title}
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-500">
                    <div className="flex flex-col">
                      <span className="font-medium">{tutorial.author.name}</span>
                      <span className="text-xs text-gray-400">{tutorial.author.email}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-500">{tutorial.categories.join(', ')}</td>
                  <td className="px-6 py-4">
                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                      tutorial.status === 'published' ? 'bg-green-100 text-green-800' : 'bg-amber-100 text-amber-800'
                    }`}>
                      {tutorial.status === 'published' ? 'Publicado' : 'En revisión'}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span className="inline-flex items-center px-3 py-1 rounded-full bg-gray-100 text-gray-800 text-sm">
                      {tutorial.contentCount}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span className="inline-flex items-center px-3 py-1 rounded-full bg-blue-100 text-blue-800 text-sm">
                      {tutorial.commentsCount}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span className="inline-flex items-center px-3 py-1 rounded-full bg-purple-100 text-purple-800 text-sm">
                      {tutorial.averageRating.toFixed(1)}/5
                    </span>
                  </td>
                  <td className="px-6 py-4 space-x-3">
                    <button 
                      onClick={() => setEditingTutorial(tutorial)}
                      className="text-primary-600 hover:text-primary-800"
                    >
                      Editar
                    </button>
                    <button 
                      onClick={() => handleDeleteTutorial(tutorial.id)}
                      className="text-red-600 hover:text-red-800"
                    >
                      Eliminar
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {editingTutorial && (
        <div className="fixed inset-0 bg-black bg-opacity-30 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-2xl">
            <div className="p-6 border-b border-gray-200 flex justify-between items-center">
              <h2 className="text-xl font-semibold text-gray-800">Editar Tutorial</h2>
              <button onClick={() => setEditingTutorial(null)} className="text-gray-400 hover:text-gray-600">
                ×
              </button>
            </div>
            <form onSubmit={handleSubmitTutorial} className="p-6 space-y-6">
              <div className="grid grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Título</label>
                  <input
                    type="text"
                    value={tutorialFormData.title}
                    onChange={(e) => setTutorialFormData({...tutorialFormData, title: e.target.value})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    required
                  />
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Estado</label>
                  <select
                    value={tutorialFormData.status}
                    onChange={(e) => setTutorialFormData({...tutorialFormData, status: e.target.value})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  >
                    <option value="published">Publicado</option>
                    <option value="draft">Borrador</option>
                    <option value="archived">Archivado</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Dificultad</label>
                  <select
                    value={tutorialFormData.difficulty}
                    onChange={(e) => setTutorialFormData({...tutorialFormData, difficulty: e.target.value})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  >
                    <option value="beginner">Principiante</option>
                    <option value="intermediate">Intermedio</option>
                    <option value="advanced">Avanzado</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Categorías</label>
                  <select
                    multiple
                    value={tutorialFormData.categoryIds}
                    onChange={(e) => setTutorialFormData({
                      ...tutorialFormData,
                      categoryIds: Array.from(e.target.selectedOptions, option => parseInt(option.value))
                    })}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500 h-32"
                  >
                    {categories.map(category => (
                      <option key={category.id} value={category.id}>{category.name}</option>
                    ))}
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Imágenes</label>
                <div className="grid grid-cols-3 gap-4 mb-4">
                  {imagePreviews.map((content) => (
                    <div key={content.id} className="relative group">
                      <img
                        src={content.contentBase64}
                        alt="Contenido del tutorial"
                        className="w-full h-32 object-cover rounded-lg border border-gray-200"
                      />
                      <button
                        type="button"
                        onClick={() => handleDeleteImage(content.id)}
                        className="absolute top-2 right-2 bg-red-500 text-white p-1 rounded-full hover:bg-red-600 transition-colors"
                      >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12"/>
                        </svg>
                      </button>
                    </div>
                  ))}
                </div>
                <input
                  type="file"
                  multiple
                  accept="image/*"
                  onChange={handleImageUpload}
                  className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-semibold file:bg-primary-50 file:text-primary-700 hover:file:bg-primary-100"
                />
              </div>

              <div className="flex justify-end gap-4 pt-6">
                <button
                  type="button"
                  onClick={() => setEditingTutorial(null)}
                  className="px-6 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className="px-6 py-2 bg-primary-500 text-white rounded-lg hover:bg-primary-600 transition-colors"
                >
                  Guardar Cambios
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-30 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md">
            <div className="p-6 border-b border-gray-200 flex justify-between items-center">
              <h2 className="text-xl font-semibold text-gray-800">Crear Nuevo Usuario</h2>
              <button onClick={() => setShowCreateModal(false)} className="text-gray-400 hover:text-gray-600">
                ×
              </button>
            </div>
            <form onSubmit={handleCreate} className="p-6 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-2">Nombre *</label>
                  <input
                    type="text"
                    value={createFormData.name}
                    onChange={(e) => setCreateFormData({...createFormData, name: e.target.value})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    required
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-2">Email *</label>
                  <input
                    type="email"
                    value={createFormData.email}
                    onChange={(e) => setCreateFormData({...createFormData, email: e.target.value})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    required
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-2">Contraseña *</label>
                  <input
                    type="password"
                    value={createFormData.password}
                    onChange={(e) => setCreateFormData({...createFormData, password: e.target.value})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    required
                    minLength="6"
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-2">Avatar URL</label>
                  <input
                    type="url"
                    value={createFormData.avatarUrl}
                    onChange={(e) => setCreateFormData({...createFormData, avatarUrl: e.target.value})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Puntos</label>
                  <input
                    type="number"
                    value={createFormData.points}
                    onChange={(e) => setCreateFormData({...createFormData, points: parseInt(e.target.value) || 0})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    min="0"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Estado</label>
                  <select
                    value={createFormData.status}
                    onChange={(e) => setCreateFormData({...createFormData, status: e.target.value})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  >
                    <option value="active">Activo</option>
                    <option value="inactive">Inactivo</option>
                  </select>
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-2">Rol</label>
                  <select
                    value={createFormData.roleId}
                    onChange={(e) => setCreateFormData({...createFormData, roleId: parseInt(e.target.value)})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  >
                    {roles.map(role => (
                      <option key={role.id} value={role.id}>{role.name}</option>
                    ))}
                  </select>
                </div>
              </div>

              <div className="flex justify-end gap-4 pt-6">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="px-6 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className="px-6 py-2 bg-primary-500 text-white rounded-lg hover:bg-primary-600 transition-colors"
                >
                  Crear Usuario
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {editingUser && (
        <div className="fixed inset-0 bg-black bg-opacity-30 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="bg-white rounded-xl shadow-2xl w-full max-w-md">
            <div className="p-6 border-b border-gray-200 flex justify-between items-center">
              <h2 className="text-xl font-semibold text-gray-800">Editar Usuario</h2>
              <button onClick={() => setEditingUser(null)} className="text-gray-400 hover:text-gray-600">
                ×
              </button>
            </div>
            <form onSubmit={handleSubmit} className="p-6 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-2">Nombre</label>
                  <input
                    type="text"
                    value={formData.name}
                    onChange={(e) => setFormData({...formData, name: e.target.value})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    required
                  />
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
                  <input
                    type="email"
                    value={formData.email}
                    onChange={(e) => setFormData({...formData, email: e.target.value})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Puntos</label>
                  <input
                    type="number"
                    value={formData.points}
                    onChange={(e) => setFormData({...formData, points: parseInt(e.target.value)})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                    min="0"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Estado</label>
                  <select
                    value={formData.status}
                    onChange={(e) => setFormData({...formData, status: e.target.value})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  >
                    <option value="active">Activo</option>
                    <option value="inactive">Inactivo</option>
                  </select>
                </div>

                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-2">Rol</label>
                  <select
                    value={formData.roleId}
                    onChange={(e) => setFormData({...formData, roleId: parseInt(e.target.value)})}
                    className="w-full px-4 py-2 rounded-lg border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
                  >
                    {roles.map(role => (
                      <option key={role.id} value={role.id}>{role.name}</option>
                    ))}
                  </select>
                </div>
              </div>

              <div className="flex justify-end gap-4 pt-6">
                <button
                  type="button"
                  onClick={() => setEditingUser(null)}
                  className="px-6 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className="px-6 py-2 bg-primary-500 text-white rounded-lg hover:bg-primary-600 transition-colors"
                >
                  Guardar Cambios
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}


