import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TodoService, TodoItem } from './services/todo.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  todos: TodoItem[] = [];
  newTitle = '';
  errorMessage = '';

  constructor(private todoService: TodoService) {}

  ngOnInit(): void {
    this.loadTodos();
  }

  loadTodos(): void {
    this.todoService.getAll().subscribe({
      next: (items) => (this.todos = items),
      error: () => (this.errorMessage = 'Could not connect to the API.')
    });
  }

  addTodo(): void {
    const title = this.newTitle.trim();
    if (!title) return;
    this.todoService.create(title).subscribe({
      next: (item) => {
        this.todos.push(item);
        this.newTitle = '';
      }
    });
  }

  toggleComplete(item: TodoItem): void {
    this.todoService.update({ ...item, isCompleted: !item.isCompleted }).subscribe({
      next: (updated) => {
        const index = this.todos.findIndex((t) => t.id === updated.id);
        if (index !== -1) this.todos[index] = updated;
      }
    });
  }

  deleteTodo(id: number): void {
    this.todoService.delete(id).subscribe({
      next: () => (this.todos = this.todos.filter((t) => t.id !== id))
    });
  }
}
