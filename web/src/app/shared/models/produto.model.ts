export interface Produto {
  id: string;
  nome: string;
  descricao: string;
  preco: number;
  quantidadeEstoque: number;
}

export interface SalvarProduto {
  nome: string;
  descricao: string | null;
  preco: number;
  quantidadeEstoque: number;
}
