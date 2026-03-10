import { createFileRoute, Link } from '@tanstack/react-router'
import { useEffect, useState } from 'react'
import {
  createCategory,
  deleteCategory,
  getCategories,
  type Category,
} from '../../services/adminCategoryService'

export const Route = createFileRoute('/admin/categories')({
  component: AdminCategoriesPage,
})

function AdminCategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([])
  const [name, setName] = useState('')
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  async function loadCategories() {
    try {
      setLoading(true)
      setError('')
      const data = await getCategories()
      setCategories(data)
    } catch (err) {
      setError('Failed to load categories.')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadCategories()
  }, [])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()

    if (!name.trim()) {
      setMessage('Category name is required.')
      return
    }

    try {
      setSubmitting(true)
      setError('')
      setMessage('')

      await createCategory(name.trim())
      setName('')
      setMessage('Category created successfully.')
      await loadCategories()
    } catch (err) {
      setMessage(
        err instanceof Error ? err.message : 'Failed to create category.'
      )
    } finally {
      setSubmitting(false)
    }
  }

  async function handleDelete(id: number) {
    const confirmed = window.confirm(
      'Are you sure you want to delete this category?'
    )

    if (!confirmed) return

    try {
      setError('')
      setMessage('')

      await deleteCategory(id)
      setMessage('Category deleted successfully.')
      await loadCategories()
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Failed to delete category.'
      )
    }
  }

  return (
    <div className="mx-auto max-w-5xl px-6 py-12">
      <div className="mb-6">
        <Link to="/admin" className="text-sm underline">
          ← Back to admin
        </Link>
      </div>

      <div className="mb-8">
        <h1 className="text-3xl font-bold">Manage categories</h1>
        <p className="mt-2 text-gray-600">
          Add new concert categories and delete existing ones.
        </p>
      </div>

      <div className="grid gap-8 lg:grid-cols-2">
        <div className="rounded-2xl border border-gray-300 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">Add category</h2>

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="mb-1 block text-sm font-medium">
                Category name
              </label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full rounded-lg border px-3 py-2"
                placeholder="Enter category name"
              />
            </div>

            <button
              type="submit"
              disabled={submitting}
              className="rounded-lg bg-black px-5 py-3 text-white hover:opacity-90 disabled:opacity-50"
            >
              {submitting ? 'Saving...' : 'Add category'}
            </button>
          </form>

          {message && <p className="mt-4 text-sm text-gray-700">{message}</p>}
          {error && <p className="mt-4 text-sm text-red-600">{error}</p>}
        </div>

        <div className="rounded-2xl border border-gray-300 bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-xl font-semibold">Existing categories</h2>

          {loading && <p>Loading categories...</p>}
          {!loading && error && <p className="text-red-600">{error}</p>}

          {!loading && !error && categories.length === 0 && (
            <p className="text-gray-600">No categories found.</p>
          )}

          {!loading && categories.length > 0 && (
            <div className="space-y-3">
              {categories.map((category) => (
                <div
                  key={category.id}
                  className="flex items-center justify-between rounded-lg border p-4"
                >
                  <div>
                    <p className="font-medium">{category.name}</p>
                    <p className="text-sm text-gray-500">ID: {category.id}</p>
                  </div>

                  <button
                    type="button"
                    onClick={() => handleDelete(category.id)}
                    className="rounded-lg bg-red-600 px-4 py-2 text-white hover:opacity-90"
                  >
                    Delete
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}