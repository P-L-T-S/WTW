import { AbstractControl, FormGroup } from '@angular/forms';

export function aplicarErrosDoServidor(form: FormGroup, campos: Record<string, string[]>): string[] {
  const naoMapeados: string[] = [];

  for (const [campo, mensagens] of Object.entries(campos)) {
    const controle = form.get(campo);

    if (controle) {
      controle.setErrors({ servidor: mensagens.join(' ') });
      controle.markAsTouched();
    } else {
      naoMapeados.push(...mensagens);
    }
  }

  return naoMapeados;
}

export function mensagemDeErro(controle: AbstractControl | null): string | null {
  if (!controle?.errors || !controle.touched) {
    return null;
  }

  const erros = controle.errors;

  if (erros['servidor']) return erros['servidor'];
  if (erros['required']) return 'Campo obrigatório.';
  if (erros['minlength']) return `Informe ao menos ${erros['minlength'].requiredLength} caracteres.`;
  if (erros['maxlength']) return `Informe no máximo ${erros['maxlength'].requiredLength} caracteres.`;
  if (erros['min']) return `O valor mínimo é ${erros['min'].min}.`;
  if (erros['max']) return `O valor máximo é ${erros['max'].max}.`;

  return 'Valor inválido.';
}
